//
// Test the generated API selectors against typos or non-existing cases
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2012-2013 Xamarin Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using System.Linq;

#if XAMCORE_2_0
using Foundation;
using ObjCRuntime;
#elif MONOMAC
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
#else
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
#endif

namespace TouchUnit.Bindings {

	public abstract class ApiSignatureTest : ApiBaseTest {

		[DllImport ("/usr/lib/libobjc.dylib")]
		// note: the returned string is not ours to free
		static extern IntPtr objc_getClass (string name);

		[DllImport ("/usr/lib/libobjc.dylib")]
		// note: the returned string is not ours to free
		static extern IntPtr method_getTypeEncoding (IntPtr method);

		[DllImport ("/usr/lib/libobjc.dylib")]
		static extern IntPtr class_getClassMethod (IntPtr klass, IntPtr selector);

		[DllImport ("/usr/lib/libobjc.dylib")]
		static extern IntPtr class_getInstanceMethod (IntPtr klass, IntPtr selector);

		protected string[] Split (string encoded, out int size)
		{
			List<string> elements = new List<string> ();
			int pos = 0;
			string s = Next (encoded, ref pos);
			int end = pos;
			while (Char.IsDigit (encoded [end]))
				end++;

			size = Int32.Parse (encoded.Substring (pos, end - pos));

			if (encoded [end] != '@' || encoded [end + 1] != '0' || encoded [end + 2] != ':') {
				if (!ContinueOnFailure)
					Assert.Fail ("Unexpected format, missing '@0:', inside '{0}'", encoded);
				return null;
			}

			pos = end + 3;

			while (s != null) {
				elements.Add (s);
				s = Next (encoded, ref pos);
			}
			return elements.ToArray ();
		}

		static string Next (string encoded, ref int pos)
		{
			// skip digits
			while (pos < encoded.Length && Char.IsDigit (encoded [pos]))
				pos++;
			if (pos >= encoded.Length)
				return null;

			StringBuilder sb = new StringBuilder ();
			int acc = 0;
			char c = encoded [pos];
			while (!Char.IsDigit (c) || acc > 0) {
				sb.Append (c);
				if (c == '{' || c == '(')
					acc++;
				else if (c == '}' || c == ')')
					acc--;
				if (++pos >= encoded.Length)
					break;
				c = encoded [pos];
			}
			return sb.ToString ();
		}

		int TypeSize (Type t)
		{
			return TypeSize (t, ref t);
		}

		int TypeSize (Type t, ref Type real)
		{
			real = t;
			if (!t.IsValueType)
				return IntPtr.Size; // platform
			if (t.IsEnum) {
				foreach (var ca in t.CustomAttributes) {
					if (ca.AttributeType.Name == "NativeAttribute")
						return IntPtr.Size;
				}
				real = Enum.GetUnderlyingType (t);
			}
			return Marshal.SizeOf (real);
		}

		protected virtual int Size (Type t, bool simd = false)
		{
			switch (t.Name) {
			// rdar 21375616 - Breaking change with EventKit[UI] enum base type
			// EventKit.EK* enums are anonymous enums in 10.10 and iOS 8, but an NSInteger in 10.11 and iOS 9.
			case "EKCalendarType":
			case "EKParticipantType":
			case "EKParticipantRole":
			case "EKParticipantStatus":
			case "EKEventStatus":
			case "EKSourceType":
			case "EKSpan":
			case "EKRecurrenceFrequency":
			case "EKEventAvailability":
				if (!IsOSX11OrIOS9)
					return 4;
				break;
			case "MDLAxisAlignedBoundingBox":
				return 32; // struct (Vector3, Vector3)
			}
			if (simd) {
				switch (t.Name) {
				case "Vector3i":	// sizeof (vector_uint3)
				case "Vector3":		// sizeof (vector_float3)
					return 16;
				case "MDLAxisAlignedBoundingBox":
					return 32; // struct (Vector3, Vector3)
				}
			}
			int size = TypeSize (t, ref t);
			return t.IsPrimitive && size < 4 ? 4 : size;
		}

		protected virtual bool Skip (Type type)
		{
			if (type.ContainsGenericParameters)
				return true;
			
			return false;
		}

		protected virtual bool Skip (Type type, MethodBase method, string selector)
		{
			return SkipDueToAttribute (method);
		}

		public int CurrentParameter { get; private set; }

		public MethodBase CurrentMethod { get; private set; }

		public string CurrentSelector { get; private set; }

		public Type CurrentType { get; private set; }

		const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

		[Test]
		public void Signatures ()
		{
			int n = 0;
			Errors = 0;
			
			foreach (Type t in Assembly.GetTypes ()) {

				var static_type = t.IsSealed && t.IsAbstract; // e.g. [Category]
				if (t.IsNested || (!static_type && !NSObjectType.IsAssignableFrom (t)))
					continue;

				if (Skip (t))
					continue;

				CurrentType = t;

				FieldInfo fi = null;
				if (!static_type)
					fi = t.GetField ("class_ptr", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
				IntPtr class_ptr = fi == null ? IntPtr.Zero : (IntPtr) fi.GetValue (null);

				foreach (MethodBase m in t.GetMethods (Flags)) 
					CheckMemberSignature (m, t, class_ptr, ref n);
				foreach (MethodBase m in t.GetConstructors (Flags)) 
					CheckMemberSignature (m, t, class_ptr, ref n);
			}
			AssertIfErrors ("{0} errors found in {1} signatures validated", Errors, n);
		}

		void CheckMemberSignature (MethodBase m, Type t, IntPtr class_ptr, ref int n)
		{
			var methodinfo = m as MethodInfo;
			var constructorinfo = m as ConstructorInfo;

			if (methodinfo == null && constructorinfo == null)
				return;

			if (m.DeclaringType != t)
				return;
			
			CurrentMethod = m;
			
			foreach (object ca in m.GetCustomAttributes (true)) {
				var exportAttribute = ca as ExportAttribute;
				if (exportAttribute == null)
					continue;
				string name = exportAttribute.Selector;
				
				if (Skip (t, m, name))
					continue;
				
				CurrentSelector = name;

				// in some cases, e.g. *Delegate, we cannot use introspection but we can still do some checks
				if (class_ptr == IntPtr.Zero) {
					BasicChecks (m, t, ref n);
				} else {
					IntrospectionTest (m, methodinfo, t, class_ptr, ref n);
				}
			}
		}

		void BasicChecks (MethodBase m, Type t, ref int n)
		{
			int native = 0;
			int pos = CurrentSelector.IndexOf (':');
			while (pos != -1) {
				native++;
				pos = CurrentSelector.IndexOf (':', pos + 1);
			}
			var mp = m.GetParameters ();
			int managed = mp.Length;
			if (t.IsSealed && t.IsAbstract) {
				// static types, e.g. [Category], adds a first 'This' argument for extension methods
				// but we also expose static properties this way, e.g. NSUrlUtilities_NSCharacterSet
				if ((managed >= 1) && (mp [0].Name == "This"))
					managed--;
			}
			if (LogProgress)
				Console.WriteLine ("{0} {1} '{2}' selector {3} : {4} == {5}", ++n, t.Name, m, CurrentSelector, native, managed);
			if (native != managed) {
				AddErrorLine ("Parameter count mismatch for {0} in {1}:{2} : Native {3} vs Managed {4}",
					CurrentSelector, t, m.Name, native, managed);
			}
		}

		void IntrospectionTest (MethodBase m, MethodInfo methodinfo, Type t, IntPtr class_ptr, ref int n)
		{
			IntPtr sel = Selector.GetHandle (CurrentSelector);
			IntPtr method;
			if (methodinfo != null)
				method = m.IsStatic ? class_getClassMethod (class_ptr, sel) :  class_getInstanceMethod (class_ptr, sel);
			else
				method = class_getInstanceMethod (class_ptr, sel);
			IntPtr tenc = method_getTypeEncoding (method);
			string encoded = Marshal.PtrToStringAuto (tenc);

			if (LogProgress)
				Console.WriteLine ("{0} {1} '{2} {3}' selector: {4} == {5}", ++n, t.Name, methodinfo != null ? methodinfo.IsStatic ? "static" : "instance" : "ctor", m, CurrentSelector, encoded);

			// NSObject has quite a bit of stuff that's not usable (except by some class that inherits from it)
			if (String.IsNullOrEmpty (encoded))
				return;

			int encoded_size = -1;
			string [] elements = null;
			try {
				elements = Split (encoded, out encoded_size);
			}
			catch {
			}
			if (elements == null) {
				if (LogProgress)
					Console.WriteLine ("[WARNING] Could not parse encoded signature for {0} : {1}", CurrentSelector, encoded);
				return;
			}

			bool result;
			CurrentParameter = 0;

			if (methodinfo != null) {
				// check return value

				result = Check (elements [CurrentParameter], methodinfo.ReturnType);
				if (!result)
					AddErrorLine ("Return Value of selector: {0} on type {1}, Type: {2}, Encoded as: {3}", CurrentSelector, t, methodinfo.ReturnType, elements [CurrentParameter]);
			}

			int size = 2 * IntPtr.Size; // self + selector (@0:)

			var parameters = m.GetParameters ();
			bool simd = (parameters.Length >= elements.Length);
			foreach (var p in parameters) {
				CurrentParameter++;
				var pt = p.ParameterType;
				if (CurrentParameter >= elements.Length) {
					// SIMD structures are not (ios8 beta2) encoded in the signature, we ignore them
					result = IgnoreSimd (CurrentSelector, t, pt);
					if (!result)
						AddErrorLine ("Selector: {0} on type {1}, Type: {2}, nothing encoded", CurrentSelector, t, pt);
				} else {
					// skip SIMD/vector parameters (as they are not encoded)
					result = IgnoreSimd (CurrentSelector, t, pt);
					if (result)
						CurrentParameter--;
					else
						result = Check (elements [CurrentParameter], pt);
					if (!result)
						AddErrorLine ("Signature failure in {1} {0} Parameter '{4}' (#{5}) is encoded as '{3}' and bound as '{2}'",
							CurrentSelector, t, pt, elements [CurrentParameter], p.Name, CurrentParameter);
				}

				size += Size (pt, simd);
			}

			// also ensure the encoded size match what MT (or XM) provides
			// catch API errors (and should catch most 64bits issues as well)
			if (size != encoded_size)
				AddErrorLine ("Size {0} != {1} for {2} on {3}: {4}", encoded_size, size, CurrentSelector, t, encoded);
		}

		static bool IgnoreSimd (string name, Type t, Type pt)
		{
			switch (pt.Name) {
			case "Vector2":
			case "Vector2i":
			case "Vector3":
			case "Vector3i":
			case "Vector4":
			case "Vector4i":
			case "MDLAxisAlignedBoundingBox": // struct { Vector3, Vector3 }
				return true;
			default:
				return false;
			}
		}

		protected virtual bool IsValidStruct (Type type, string structName)
		{
			switch (structName) {
			// MKPolygon 'static MonoTouch.MapKit.MKPolygon _FromPoints(IntPtr, Int32)' selector: polygonWithPoints:count: == @16@0:4^{?=dd}8I12
			// NSValue 'static MonoTouch.Foundation.NSValue FromCMTime(CMTime)' selector: valueWithCMTime: == @32@0:4{?=qiIq}8
			case "?":
				return type.IsValueType; // || (type.FullName == "System.IntPtr");
#if XAMCORE_2_0
			case "CGRect":
				return type.FullName == "CoreGraphics.CGRect";
			case "CGSize":
				return type.FullName == "CoreGraphics.CGSize";
			case "CGPoint":
				return type.FullName == "CoreGraphics.CGPoint";
#else
			case "CGRect":
				return type.FullName == "System.Drawing.RectangleF";
			case "CGSize":
				return type.FullName == "System.Drawing.SizeF";
			case "CGPoint":
				return type.FullName == "System.Drawing.PointF";
#endif
			case "opaqueCMFormatDescription":
				switch (type.Name) {
				case "CMFormatDescription":
				case "CMVideoFormatDescription":
				case "CMAudioFormatDescription":
					return true;
				}
				break;
			case "opaqueCMSampleBuffer":
				structName = "CMSampleBuffer";
				break;
			case "_NSRange":
				structName = "NSRange";
				break;
			// textureWithContentsOfFile:options:queue:completionHandler: == v24@0:4@8@12^{dispatch_queue_s=}16@?20
			case "dispatch_queue_s":
				structName = "DispatchQueue";
				break;
			case "OpaqueCMClock":
				structName = "CMClock";
				break;
			case "OpaqueCMTimebase":
				structName = "CMTimebase";
				break;
			case "__CFRunLoop":
				structName = "CFRunLoop";
				break;
			case "_GLKVector4":
				structName = "Vector4";
				break;
			case "_GLKVector3":
				structName = "Vector3";
				break;
			case "_GLKVector2":
				structName = "Vector2";
				break;
			case "_GLKMatrix2":
				structName = "Matrix2";
				break;
			case "_GLKMatrix3":
				structName = "Matrix3";
				break;
			case "_GLKMatrix4":
				structName = "Matrix4";
				break;
			case "__CVPixelBufferPool":
				structName = "CVPixelBufferPool";
				break;
			case "opaqueMTAudioProcessingTap":
				structName = "MTAudioProcessingTap";
				break;
			case "OpaqueMIDIEndpoint":
				structName = "Int32";
				break;
			case "__CFDictionary":
				structName = "NSDictionary";
				break;
			case "__CFUUID":
				// CBAttribute.UUID is defined as a CBUUID but ObjC runtime tell us it's __CFUUID
				// which makes it sound like a (undocumented) toll free bridged type
				structName = "CBUUID";
				break;
			case "__CFString":
				if (type.FullName == "System.String")
					return true;
				break;
#if !MONOMAC
			// definition is different on OSX
			case "SCNVector4":
				switch (type.Name) {
				case "SCNVector4":
				// typedef SCNVector4 SCNQuaternion; (SceneKitTypes.h)
				case "SCNQuaternion":
					return true;
				}
				break;
#endif
			case "_CGLContextObject":
				structName = "CGLContext";
				break;
			case "_CGLPixelFormatObject":
				structName = "CGLPixelFormat";
				break;
			case "OpaqueSecIdentityRef":
				structName = "SecIdentity";
				break;
			case "__SecTrust":
				structName = "SecTrust";
				break;
			case "_NSZone":
				structName = "NSZone";
				break;
			case "_AVBeatRange":
				structName = "AVBeatRange";
				break;
			case "AVAudio3DPoint":
				structName = "Vector3";
				break;
			case "OpaqueMusicSequence":
				structName = "MusicSequence";
				break;
			case "OpaqueAudioComponentInstance":
				structName = "AudioUnit";
				break;
			case "OpaqueAudioComponent":
				structName = "AudioComponent";
				break;
			case "GCQuaternion":
				structName = "Quaterniond"; // OpenTK.Quaterniond
				break;
			case "OpaqueSecAccessControl": // El Capitan
			case "__SecAccessControl":
				structName = "SecAccessControl";
				break;
			case "AudioChannelLayout":
				// this is actually an `nint` used as a pointer (to get a unique signature for the .ctor)
				// there's custom code in src/AVFoundation/AVAudioChannelLayout.cs to deal with this
#if XAMCORE_2_0
				structName = "nint";
#else
				structName = "Int32";
#endif
				break;
#if !XAMCORE_2_0
			// in compat it's a class (instead of a struct) hence this hack
			case "AudioComponentDescription":
				structName = "AudioComponentDescriptionNative";
				break;
#endif
			}
			return type.Name == structName;
		}

		static Type inativeobject = typeof (INativeObject);

		protected virtual bool Check (string encodedType, Type type)
		{
			char c = encodedType [0];

			if (encodedType.Length == 1)
				return Check (c, type);

			switch (c) {
			// GLKBaseEffect 'instance Vector4 get_LightModelAmbientColor()' selector: lightModelAmbientColor == (_GLKVector4={?=ffff}{?=ffff}{?=ffff}[4f])8@0:4
			case '(':
			case '{':
				string struct_name = encodedType.Substring (1, encodedType.IndexOf ('=') - 1);
				return IsValidStruct (type, struct_name);
			case '@':
				switch (encodedType [1]) {
				case '?':
					return (type.Name == "NSAction") || type.BaseType.FullName == "System.MulticastDelegate";
				default:
					return false;
				}
			case '^':
				switch (encodedType [1]) {
				case 'v':
					// NSOpenGLContext 'instance MonoMac.OpenGL.CGLContext get_CGLContext()' selector: CGLContextObj == ^v8@0:4
					if ((CurrentType.Name == "NSOpenGLContext") && (type.Name == "CGLContext"))
						return true;
					// NSOpenGLPixelFormat 'instance MonoMac.OpenGL.CGLPixelFormat get_CGLPixelFormat()' selector: CGLPixelFormatObj == ^v8@0:4
					if ((CurrentType.Name == "NSOpenGLPixelFormat") && (type.Name == "CGLPixelFormat"))
						return true;
					if (type.Name == "ABRecord") {
						if ((CurrentType.Name == "EKParticipant") || CurrentType.Name.StartsWith ("PKPayment", StringComparison.OrdinalIgnoreCase))
							return true;
					}
					if ((type.Name == "ABAddressBook") && (CurrentType.Name == "EKParticipant"))
						return true;
					return (type.FullName == "System.IntPtr");
				case 'B':
				case 'd':
				case 'f':
				case 'I':
				case 'i':
				case 'c':
				case 'q':
				case 'Q':
				case 'S':
					return (type.FullName == "System.IntPtr") || Check (encodedType.Substring (1), type.GetElementType ());
				// NSInputStream 'instance Boolean GetBuffer(IntPtr ByRef, UInt32 ByRef)' selector: getBuffer:length: == c16@0:4^*8^I12
				case '*':
				case '{':
				// 10.7 only: NSArray 'static MonoMac.Foundation.NSArray FromObjects(IntPtr, Int32)' selector: arrayWithObjects:count: == @16@0:4^r@8I12
				case 'r':
					if (type.FullName == "System.IntPtr")
						return true;
					return Check (encodedType.Substring (1), type.IsByRef ? type.GetElementType () : type);
				case '@':
					return Check ('@', type.IsByRef ? type.GetElementType () : type);
				case '^':
				case '?':
					return (type.FullName == "System.IntPtr");
				default:
					return false;
				}
			case 'r':
				// const -> ignore
				// e.g. vectorWithValues:count: == @16@0:4r^f8L12
			case 'o':
				// out -> ignore
				// e.g. validateValue:forKey:error: == c20@0:4N^@8@12o^@16
			case 'N':
				// inout -> ignore
				// e.g. validateValue:forKey:error: == c20@0:4N^@8@12o^@16
			case 'V':
				// oneway -> ignore
				// e.g. NSObject 'instance Void NativeRelease()' selector: release == Vv8@0:4
				return Check (encodedType.Substring (1), type);
			default:
				return false;
			}
		}

		/// <summary>
		/// Check that specified encodedType match the type and caller.
		/// </summary>
		/// <param name="encodedType">Encoded type from the ObjC signature.</param>
		/// <param name="type">Managed type representing the encoded type.</param>
		/// <param name="caller">Caller's type. Useful to limit any special case.</param>
		protected virtual bool Check (char encodedType, Type type)
		{
			switch (encodedType) {
			case '@':
				return (type.IsInterface ||								// protocol
					type.IsArray || 									// NSArray
					(type.Name == "NSArray") || 						// NSArray
					(type.FullName == "System.String") || 						// NSString
					(type.FullName == "System.IntPtr") || 						// unbinded, e.g. internal
					(type.BaseType.FullName == "System.MulticastDelegate") || 	// completion handler -> delegate
					NSObjectType.IsAssignableFrom (type)) ||					// NSObject derived
					inativeobject.IsAssignableFrom (type);						// e.g. CGImage
			case 'B':
				// 64 bits only encode this
				return type.FullName == "System.Boolean";
			case 'c': // char, used for C# bool
				switch (type.FullName) {
				case "System.Boolean":
				case "System.SByte":
					return true;
				default:
					return type.IsEnum && TypeSize (type) == 1;
				}
			case 'C':
				switch (type.FullName) {
				case "System.Byte":
				// GLKBaseEffect 'instance Boolean get_ColorMaterialEnabled()' selector: colorMaterialEnabled == C8@0:4
				case "System.Boolean":
					return true;
				default:
					return false;
				}
			case 'd':
				switch (type.FullName) {
				case "System.Double":
					return true;
				case "System.nfloat":
					return IntPtr.Size == 8;
				default:
					return false;
				}
			case 'f':
				switch (type.FullName) {
				case "System.Single":
					return true;
				case "System.nfloat":
					return IntPtr.Size == 4;
				default:
					return false;
				}
			case 'i':
				switch (type.FullName) {
				case "System.Int32":
					return true;
				case "System.nint":
					return IntPtr.Size == 4;
				case "EventKit.EKSourceType":
				case "EventKit.EKCalendarType":
				case "EventKit.EKEventAvailability":
				case "EventKit.EKEventStatus":
				case "EventKit.EKParticipantRole":
				case "EventKit.EKParticipantStatus":
				case "EventKit.EKParticipantType":
				case "EventKit.EKRecurrenceFrequency":
				case "EventKit.EKSpan":
				case "EventKit.EKAlarmType":
				// EventKit.EK* enums are anonymous enums in 10.10 and iOS 8, but an NSInteger in 10.11 and iOS 9.
					if (IsOSX11OrIOS9)
						goto default;
					return true;
				default:
					return type.IsEnum && TypeSize (type) == 4;
				}
			case 'I':
				switch (type.FullName) {
				case "System.UInt32":
					return true;
				case "System.nint": // check
				case "System.nuint":
					return IntPtr.Size == 4;
				default:
					return type.IsEnum && TypeSize (type) == 4;
				}
			case 'l':
				switch (type.FullName) {
				case "System.Int32":
					return true;
				case "System.nint":
					return IntPtr.Size == 4;
				default:
					return type.IsEnum && TypeSize (type) == 4;
				}
			case 'L':
				switch (type.FullName) {
				case "System.UInt32":
					return true;
				case "System.nint": // check
				case "System.nuint":
					return IntPtr.Size == 4;
				default:
					return type.IsEnum && TypeSize (type) == 4;
				}
			case 'q':
				switch (type.FullName) {
				case "System.Int64":
					return true;
				case "System.nint":
					return IntPtr.Size == 8;
				default:
					return type.IsEnum && TypeSize (type) == 8;
				}
			case 'Q':
				switch (type.FullName) {
				case "System.UInt64":
					return true;
				case "System.nint": // check
				case "System.nuint":
					return IntPtr.Size == 8;
				default:
					return type.IsEnum && TypeSize (type) == 8;
				}
			case 's':
				return type.FullName == "System.Int16";
			// unsigned 16 bits
			case 'S':
				switch (type.FullName) {
				case "System.UInt16":
				// NSString 'instance Char _characterAtIndex(Int32)' selector: characterAtIndex: == S12@0:4I8
				case "System.Char":
					return true;
				default:
					return type.IsEnum && TypeSize (type) == 2;
				}
			case ':':
				return type.Name == "Selector";
			case 'v':
				return type.FullName == "System.Void";
			case '?':
				return type.BaseType.FullName == "System.MulticastDelegate";	// completion handler -> delegate
			case '#':
				return type.FullName == "System.IntPtr" || type.Name == "Class";
			// CAMediaTimingFunction 'instance Void GetControlPointAtIndex(Int32, IntPtr)' selector: getControlPointAtIndex:values: == v16@0:4L8[2f]12
			case '[':
				return type.FullName == "System.IntPtr";
			// const uint8_t * -> IntPtr
			// NSCoder 'instance Void EncodeBlock(IntPtr, Int32, System.String)' selector: encodeBytes:length:forKey: == v20@0:4r*8I12@16
			case '*':
				return type.FullName == "System.IntPtr";
			case '^':
				return type.FullName == "System.IntPtr";
			}
			return false;
		}
	}
}
