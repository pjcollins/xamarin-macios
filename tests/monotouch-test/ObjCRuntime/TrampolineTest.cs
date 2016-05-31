using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
#if XAMCORE_2_0
using Foundation;
using ObjCRuntime;
#if !__TVOS__
using MapKit;
#endif
#if !__WATCHOS__
using CoreAnimation;
#endif
using CoreLocation;
#if !__WATCHOS__
using CoreMedia;
#endif
#else
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.MapKit;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreLocation;
using MonoTouch.CoreMedia;
#endif
using OpenTK;
using NUnit.Framework;

#if XAMCORE_2_0
using RectangleF=CoreGraphics.CGRect;
using SizeF=CoreGraphics.CGSize;
using PointF=CoreGraphics.CGPoint;
#else
using nfloat=global::System.Single;
using nint=global::System.Int32;
using nuint=global::System.UInt32;
#endif

namespace MonoTouchFixtures.ObjCRuntime {
	
	[TestFixture]
	[Preserve (AllMembers = true)]
	public class TrampolineTest {
		public static readonly nfloat pi = 3.14159f;

		public bool IsSim64 { get { return IntPtr.Size == 8 && Runtime.Arch == Arch.SIMULATOR; } }
		public bool IsSim32 { get { return IntPtr.Size == 4 && Runtime.Arch == Arch.SIMULATOR; } }
		public bool IsArm64 { get { return IntPtr.Size == 8 && Runtime.Arch == Arch.DEVICE; } }
		public bool IsArm32 { get { return IntPtr.Size == 4 && Runtime.Arch == Arch.DEVICE; } }

#if !__WATCHOS__
		[Test]
		public void StretTrampolineTest ()
		{
			CMTimeRange tr;
			using (var obj = new StretTrampolines ()) {
				if (IsArm64) {
					tr = CMTimeRange_objc_msgSend (obj.Handle, Selector.GetHandle ("myTimeRange"));
				} else {
					CMTimeRange_objc_msgSend (out tr, obj.Handle, Selector.GetHandle ("myTimeRange"));
				}
			}
			Assert.AreEqual (12, tr.Duration.Value);
			Assert.AreEqual (1, tr.Duration.TimeScale);
			Assert.AreEqual (1, tr.Start.Value);
			Assert.AreEqual (1, tr.Start.TimeScale);
		}
#endif // !__WATCHOS__

		[Test]
		public void StretIIIITrampolineTest ()
		{
			StretTrampolines obj = new StretTrampolines ();
			IntPtr class_ptr = Class.GetHandle ("StretTrampolines");
			IIIIStruct rv = new IIIIStruct ();
			double rvd;
			float rvf;

			if (IsSim64 || IsArm64) {
				rv = IIIIStruct_objc_msgSend (obj.Handle, new Selector ("Test_IIIIStruct").Handle);
			} else {
				IIIIStruct_objc_msgSend_stret (out rv, obj.Handle, new Selector ("Test_IIIIStruct").Handle);
			}			 
			Assert.That ("[1;2;3;4]" == rv.ToString ());
			if (IsSim64 || IsArm64) {
				rv = IIIIStruct_objc_msgSend (class_ptr, new Selector ("Test_StaticIIIIStruct").Handle);
			} else {
				IIIIStruct_objc_msgSend_stret (out rv, class_ptr, new Selector ("Test_StaticIIIIStruct").Handle);
			}
			Assert.That ("[10;20;30;40]" == rv.ToString ());
			if (IsSim64 || IsArm64) {
				rv = IIIIStruct_objc_msgSend (obj.Handle, new Selector ("Test_IIIIStructProperty").Handle);
			} else {
				IIIIStruct_objc_msgSend_stret (out rv, obj.Handle, new Selector ("Test_IIIIStructProperty").Handle);
			}
			Assert.That ("[100;200;300;400]" == rv.ToString ());
			if (IsSim64 || IsArm64) {
				rv = IIIIStruct_objc_msgSend (class_ptr, new Selector ("Test_StaticIIIIStructProperty").Handle);
			} else {
				IIIIStruct_objc_msgSend_stret (out rv, class_ptr, new Selector ("Test_StaticIIIIStructProperty").Handle);
			}
			Assert.That ("[1000;2000;3000;4000]" == rv.ToString ());
			
			rvd = rvf = 0;
			if (IsSim64 || IsArm64) {
				rv = IIIIStruct_objc_msgSend_out_float (class_ptr, new Selector ("Test_StaticIIIIStruct_out_Float:").Handle, out rvf);
			} else {
				IIIIStruct_objc_msgSend_stret_out_float (out rv, class_ptr, new Selector ("Test_StaticIIIIStruct_out_Float:").Handle, out rvf);
			}
			//Console.WriteLine ("Got: {0} and {1} and {2}", rv.ToString (), rvf, rvd);
			Assert.That ("[10;20;300;4000]" == rv.ToString ());
			Assert.That (rvf == 3.15f);
			
			rvd = rvf = 0;
			if (IsSim64 || IsArm64) {
				rv = IIIIStruct_objc_msgSend_out_double (obj.Handle, new Selector ("Test_IIIIStruct_out_Double:").Handle, out rvd);
			} else {
				IIIIStruct_objc_msgSend_stret_out_double (out rv, obj.Handle, new Selector ("Test_IIIIStruct_out_Double:").Handle, out rvd);
			}
			//Console.WriteLine ("Got: {0} and {1} and {2}", rv.ToString (), rvf, rvd);
			Assert.That ("[1;20;300;4000]" == rv.ToString ());
			Assert.That (rvd == 3.14);
			
		}

		[Test]
		public void StretFFFFTrampolineTest ()
		{
			StretTrampolines obj = new StretTrampolines ();
			IntPtr class_ptr = Class.GetHandle ("StretTrampolines");
			var rv = new FFFFStruct ();
			double rvd;
			float rvf;

			if (IsSim64 || IsArm64) {
				rv = FFFFStruct_objc_msgSend (obj.Handle, new Selector ("Test_FFFFStruct").Handle);
			} else {
				FFFFStruct_objc_msgSend_stret (out rv, obj.Handle, new Selector ("Test_FFFFStruct").Handle);
			}			 
			Assert.That ("[1;2;3;4]" == rv.ToString ());
			if (IsSim64 || IsArm64) {
				rv = FFFFStruct_objc_msgSend (class_ptr, new Selector ("Test_StaticFFFFStruct").Handle);
			} else {
				FFFFStruct_objc_msgSend_stret (out rv, class_ptr, new Selector ("Test_StaticFFFFStruct").Handle);
			}
			Assert.That ("[10;20;30;40]" == rv.ToString ());
			if (IsSim64 || IsArm64) {
				rv = FFFFStruct_objc_msgSend (obj.Handle, new Selector ("Test_FFFFStructProperty").Handle);
			} else {
				FFFFStruct_objc_msgSend_stret (out rv, obj.Handle, new Selector ("Test_FFFFStructProperty").Handle);
			}
			Assert.That ("[100;200;300;400]" == rv.ToString ());
			if (IsSim64 || IsArm64) {
				rv = FFFFStruct_objc_msgSend (class_ptr, new Selector ("Test_StaticFFFFStructProperty").Handle);
			} else {
				FFFFStruct_objc_msgSend_stret (out rv, class_ptr, new Selector ("Test_StaticFFFFStructProperty").Handle);
			}
			Assert.That ("[1000;2000;3000;4000]" == rv.ToString ());

			rvd = rvf = 0;
			if (IsSim64 || IsArm64) {
				rv = FFFFStruct_objc_msgSend_out_float (class_ptr, new Selector ("Test_StaticFFFFStruct_out_Float:").Handle, out rvf);
			} else {
				FFFFStruct_objc_msgSend_stret_out_float (out rv, class_ptr, new Selector ("Test_StaticFFFFStruct_out_Float:").Handle, out rvf);
			}
			//Console.WriteLine ("Got: {0} and {1} and {2}", rv.ToString (), rvf, rvd);
			Assert.That ("[10;20;300;4000]" == rv.ToString ());
			Assert.That (rvf == 3.15f);

			rvd = rvf = 0;
			if (IsSim64 || IsArm64) {
				rv = FFFFStruct_objc_msgSend_out_double (obj.Handle, new Selector ("Test_FFFFStruct_out_Double:").Handle, out rvd);
			} else {
				FFFFStruct_objc_msgSend_stret_out_double (out rv, obj.Handle, new Selector ("Test_FFFFStruct_out_Double:").Handle, out rvd);
			}
			//Console.WriteLine ("Got: {0} and {1} and {2}", rv.ToString (), rvf, rvd);
			Assert.That ("[1;20;300;4000]" == rv.ToString ());
			Assert.That (rvd == 3.14);
		}

		[Test]
		public void StretFIFITrampolineTest ()
		{
			StretTrampolines obj = new StretTrampolines ();
			IntPtr class_ptr = Class.GetHandle ("StretTrampolines");
			var rv = new FIFIStruct ();
			double rvd;
			float rvf;

			if (IsSim64 || IsArm64) {
				rv = FIFIStruct_objc_msgSend (obj.Handle, new Selector ("Test_FIFIStruct").Handle);
			} else {
				FIFIStruct_objc_msgSend_stret (out rv, obj.Handle, new Selector ("Test_FIFIStruct").Handle);
			}			 
			Assert.That ("[1;2;3;4]" == rv.ToString ());
			if (IsSim64 || IsArm64) {
				rv = FIFIStruct_objc_msgSend (class_ptr, new Selector ("Test_StaticFIFIStruct").Handle);
			} else {
				FIFIStruct_objc_msgSend_stret (out rv, class_ptr, new Selector ("Test_StaticFIFIStruct").Handle);
			}
			Assert.That ("[10;20;30;40]" == rv.ToString ());
			if (IsSim64 || IsArm64) {
				rv = FIFIStruct_objc_msgSend (obj.Handle, new Selector ("Test_FIFIStructProperty").Handle);
			} else {
				FIFIStruct_objc_msgSend_stret (out rv, obj.Handle, new Selector ("Test_FIFIStructProperty").Handle);
			}
			Assert.That ("[100;200;300;400]" == rv.ToString ());
			if (IsSim64 || IsArm64) {
				rv = FIFIStruct_objc_msgSend (class_ptr, new Selector ("Test_StaticFIFIStructProperty").Handle);
			} else {
				FIFIStruct_objc_msgSend_stret (out rv, class_ptr, new Selector ("Test_StaticFIFIStructProperty").Handle);
			}
			Assert.That ("[1000;2000;3000;4000]" == rv.ToString ());

			rvd = rvf = 0;
			if (IsSim64 || IsArm64) {
				rv = FIFIStruct_objc_msgSend_out_float (class_ptr, new Selector ("Test_StaticFIFIStruct_out_Float:").Handle, out rvf);
			} else {
				FIFIStruct_objc_msgSend_stret_out_float (out rv, class_ptr, new Selector ("Test_StaticFIFIStruct_out_Float:").Handle, out rvf);
			}
			//Console.WriteLine ("Got: {0} and {1} and {2}", rv.ToString (), rvf, rvd);
			Assert.That ("[10;20;300;4000]" == rv.ToString ());
			Assert.That (rvf == 3.15f);

			rvd = rvf = 0;
			if (IsSim64 || IsArm64) {
				rv = FIFIStruct_objc_msgSend_out_double (obj.Handle, new Selector ("Test_FIFIStruct_out_Double:").Handle, out rvd);
			} else {
				FIFIStruct_objc_msgSend_stret_out_double (out rv, obj.Handle, new Selector ("Test_FIFIStruct_out_Double:").Handle, out rvd);
			}
			//Console.WriteLine ("Got: {0} and {1} and {2}", rv.ToString (), rvf, rvd);
			Assert.That ("[1;20;300;4000]" == rv.ToString ());
			Assert.That (rvd == 3.14);
		}

		[Test]
		public void StretIFIFTrampolineTest ()
		{
			StretTrampolines obj = new StretTrampolines ();
			IntPtr class_ptr = Class.GetHandle ("StretTrampolines");
			var rv = new IFIFStruct ();
			double rvd;
			float rvf;

			if (IsSim64 || IsArm64) {
				rv = IFIFStruct_objc_msgSend (obj.Handle, new Selector ("Test_IFIFStruct").Handle);
			} else {
				IFIFStruct_objc_msgSend_stret (out rv, obj.Handle, new Selector ("Test_IFIFStruct").Handle);
			}			 
			Assert.That ("[1;2;3;4]" == rv.ToString ());
			if (IsSim64 || IsArm64) {
				rv = IFIFStruct_objc_msgSend (class_ptr, new Selector ("Test_StaticIFIFStruct").Handle);
			} else {
				IFIFStruct_objc_msgSend_stret (out rv, class_ptr, new Selector ("Test_StaticIFIFStruct").Handle);
			}
			Assert.That ("[10;20;30;40]" == rv.ToString ());
			if (IsSim64 || IsArm64) {
				rv = IFIFStruct_objc_msgSend (obj.Handle, new Selector ("Test_IFIFStructProperty").Handle);
			} else {
				IFIFStruct_objc_msgSend_stret (out rv, obj.Handle, new Selector ("Test_IFIFStructProperty").Handle);
			}
			Assert.That ("[100;200;300;400]" == rv.ToString ());
			if (IsSim64 || IsArm64) {
				rv = IFIFStruct_objc_msgSend (class_ptr, new Selector ("Test_StaticIFIFStructProperty").Handle);
			} else {
				IFIFStruct_objc_msgSend_stret (out rv, class_ptr, new Selector ("Test_StaticIFIFStructProperty").Handle);
			}
			Assert.That ("[1000;2000;3000;4000]" == rv.ToString ());

			rvd = rvf = 0;
			if (IsSim64 || IsArm64) {
				rv = IFIFStruct_objc_msgSend_out_float (class_ptr, new Selector ("Test_StaticIFIFStruct_out_Float:").Handle, out rvf);
			} else {
				IFIFStruct_objc_msgSend_stret_out_float (out rv, class_ptr, new Selector ("Test_StaticIFIFStruct_out_Float:").Handle, out rvf);
			}
			//Console.WriteLine ("Got: {0} and {1} and {2}", rv.ToString (), rvf, rvd);
			Assert.That ("[10;20;300;4000]" == rv.ToString ());
			Assert.That (rvf == 3.15f);

			rvd = rvf = 0;
			if (IsSim64 || IsArm64) {
				rv = IFIFStruct_objc_msgSend_out_double (obj.Handle, new Selector ("Test_IFIFStruct_out_Double:").Handle, out rvd);
			} else {
				IFIFStruct_objc_msgSend_stret_out_double (out rv, obj.Handle, new Selector ("Test_IFIFStruct_out_Double:").Handle, out rvd);
			}
			//Console.WriteLine ("Got: {0} and {1} and {2}", rv.ToString (), rvf, rvd);
			Assert.That ("[1;20;300;4000]" == rv.ToString ());
			Assert.That (rvd == 3.14);
		}



		[Test]
		public void DoubleReturnTest ()
		{
			DoubleStretTrampolines obj = new DoubleStretTrampolines ();
			IntPtr class_ptr = Class.GetHandle ("DoubleStretTrampolines");

			Assert.That (0 == Messaging.Double_objc_msgSend (obj.Handle, new Selector ("Test_Zero").Handle), "#1");
			Assert.That (0 == Messaging.Double_objc_msgSend (class_ptr, new Selector ("Test_StaticZero").Handle), "#2");
			Assert.That (0 == Messaging.Double_objc_msgSend (obj.Handle, new Selector ("Test_ZeroProperty").Handle), "#3");
			Assert.That (0 == Messaging.Double_objc_msgSend (class_ptr, new Selector ("Test_StaticZeroProperty").Handle), "#4");
		}

		[Test]
		public void FloatReturnTest ()
		{
			FloatStretTrampolines obj = new FloatStretTrampolines ();
			IntPtr class_ptr = Class.GetHandle ("FloatStretTrampolines");

			Assert.That (0 == Messaging.float_objc_msgSend (obj.Handle, new Selector ("Test_Zero").Handle), "#1");
			Assert.That (0 == Messaging.float_objc_msgSend (class_ptr, new Selector ("Test_StaticZero").Handle), "#2");
			Assert.That (0 == Messaging.float_objc_msgSend (obj.Handle, new Selector ("Test_ZeroProperty").Handle), "#3");
			Assert.That (0 == Messaging.float_objc_msgSend (class_ptr, new Selector ("Test_StaticZeroProperty").Handle), "#4");
		}
		
		[Test]
		public void LoooongTest ()
		{
			LongTrampolines obj = new LongTrampolines ();
			long a = 3;
			long b = 5;
			long c = 15;
			long d = 29;
			long e;
			
			void_objc_msgSend_long_long_ref_long_ref_long (obj.Handle, new Selector ("ManyLongs:b:c:d:").Handle, a, b, out c, out d);
			Assert.That (c == a + b, "#a1");
			Assert.That (d == b - a, "#a2");
			
			c = 15;
			d = 29;
			
			e = long_objc_msgSend_long_long_out_long_out_long (obj.Handle, new Selector ("VeryManyLongs:b:c:d:").Handle, a, b, out c, out d);
			Assert.That (a + b + c + d == e, "#b1");
			Assert.That (c == a + b, "#b2");
			Assert.That (d == a - b, "#b3");
		}
		
		const string LIBOBJC_DYLIB = "/usr/lib/libobjc.dylib";

#if !__WATCHOS__
		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		extern static OpenTK.Matrix4 Matrix4_objc_msgSend (IntPtr receiver, IntPtr selector);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend_stret")]
		extern static void Matrix4_objc_msgSend_stret (out OpenTK.Matrix4 retval, IntPtr receiver, IntPtr selector);
#endif // !__WATCHOS__

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend_stret")]
		extern static void IIIIStruct_objc_msgSend_stret (out IIIIStruct retval, IntPtr receiver, IntPtr selector);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		extern static IIIIStruct IIIIStruct_objc_msgSend (IntPtr receiver, IntPtr selector);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend_stret")]
		extern static void IIIIStruct_objc_msgSend_stret_out_double (out IIIIStruct retval, IntPtr receiver, IntPtr selector, out double arg1);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		extern static IIIIStruct IIIIStruct_objc_msgSend_out_double (IntPtr receiver, IntPtr selector, out double arg1);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend_stret")]
		extern static void IIIIStruct_objc_msgSend_stret_out_float (out IIIIStruct retval, IntPtr receiver, IntPtr selector, out float arg1);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		extern static IIIIStruct IIIIStruct_objc_msgSend_out_float (IntPtr receiver, IntPtr selector, out float arg1);


		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend_stret")]
		extern static void FFFFStruct_objc_msgSend_stret (out FFFFStruct retval, IntPtr receiver, IntPtr selector);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		extern static FFFFStruct FFFFStruct_objc_msgSend (IntPtr receiver, IntPtr selector);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend_stret")]
		extern static void FFFFStruct_objc_msgSend_stret_out_double (out FFFFStruct retval, IntPtr receiver, IntPtr selector, out double arg1);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		extern static FFFFStruct FFFFStruct_objc_msgSend_out_double (IntPtr receiver, IntPtr selector, out double arg1);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend_stret")]
		extern static void FFFFStruct_objc_msgSend_stret_out_float (out FFFFStruct retval, IntPtr receiver, IntPtr selector, out float arg1);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		extern static FFFFStruct FFFFStruct_objc_msgSend_out_float (IntPtr receiver, IntPtr selector, out float arg1);


		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend_stret")]
		extern static void IFIFStruct_objc_msgSend_stret (out IFIFStruct retval, IntPtr receiver, IntPtr selector);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		extern static IFIFStruct IFIFStruct_objc_msgSend (IntPtr receiver, IntPtr selector);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend_stret")]
		extern static void IFIFStruct_objc_msgSend_stret_out_double (out IFIFStruct retval, IntPtr receiver, IntPtr selector, out double arg1);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		extern static IFIFStruct IFIFStruct_objc_msgSend_out_double (IntPtr receiver, IntPtr selector, out double arg1);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend_stret")]
		extern static void IFIFStruct_objc_msgSend_stret_out_float (out IFIFStruct retval, IntPtr receiver, IntPtr selector, out float arg1);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		extern static IFIFStruct IFIFStruct_objc_msgSend_out_float (IntPtr receiver, IntPtr selector, out float arg1);


		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend_stret")]
		extern static void FIFIStruct_objc_msgSend_stret (out FIFIStruct retval, IntPtr receiver, IntPtr selector);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		extern static FIFIStruct FIFIStruct_objc_msgSend (IntPtr receiver, IntPtr selector);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend_stret")]
		extern static void FIFIStruct_objc_msgSend_stret_out_double (out FIFIStruct retval, IntPtr receiver, IntPtr selector, out double arg1);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		extern static FIFIStruct FIFIStruct_objc_msgSend_out_double (IntPtr receiver, IntPtr selector, out double arg1);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend_stret")]
		extern static void FIFIStruct_objc_msgSend_stret_out_float (out FIFIStruct retval, IntPtr receiver, IntPtr selector, out float arg1);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		extern static FIFIStruct FIFIStruct_objc_msgSend_out_float (IntPtr receiver, IntPtr selector, out float arg1);


		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend_stret")]
		extern static void double_objc_msgSend_stret_out_double (out double retval, IntPtr receiver, IntPtr selector, out double arg1);
		
		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend_stret")]
		extern static void double_objc_msgSend_stret_out_float (out double retval, IntPtr receiver, IntPtr selector, out float arg1);
		
		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend_stret")]
		extern static void float_objc_msgSend_stret_out_double (out float retval, IntPtr receiver, IntPtr selector, out double arg1);
		
		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend_stret")]
		extern static void float_objc_msgSend_stret_out_float (out float retval, IntPtr receiver, IntPtr selector, out float arg1);
		
		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend_stret")]
		extern static void PointF_objc_msgSend_stret_out_double (out PointF retval, IntPtr receiver, IntPtr selector, out double arg1);
		
		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend_stret")]
		extern static void PointF_objc_msgSend_stret_out_float (out PointF retval, IntPtr receiver, IntPtr selector, out float arg1);
		
		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend_stret")]
		extern static void RectangleF_objc_msgSend_stret_IntPtr_IntPtr_RectangleF (out RectangleF retval, IntPtr receiver, IntPtr selector, IntPtr a, IntPtr b, RectangleF c);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		extern static RectangleF RectangleF_objc_msgSend_IntPtr_IntPtr_RectangleF (IntPtr receiver, IntPtr selector, IntPtr a, IntPtr b, RectangleF c);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		extern static void void_objc_msgSend_out_NSError (IntPtr receiver, IntPtr selector, out IntPtr error, int arg1);
		
		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		extern static void void_objc_msgSend_long_long_ref_long_ref_long (IntPtr receiver, IntPtr selector, long arg1, long arg2, out long arg3, out long arg4);
			             
		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		extern static long long_objc_msgSend_long_long_out_long_out_long (IntPtr receiver, IntPtr selector, long arg1, long arg2, out long arg3, out long arg4);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		extern static bool bool_objc_msgSend_out_PointF (IntPtr receiver, IntPtr selector, out PointF point);


#if !__WATCHOS__
		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		extern static CMTimeRange CMTimeRange_objc_msgSend (IntPtr receiver, IntPtr selector);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend_stret")]
		extern static void CMTimeRange_objc_msgSend (out CMTimeRange retval, IntPtr receiver, IntPtr selector);
#endif // !__WATCHOS__

		void AreAlmostEqual (RectangleF left, RectangleF right, string message)
		{
			var delta = 0.000001f;
			Assert.AreEqual (left.X, right.X, delta, message);
			Assert.AreEqual (left.Y, right.Y, delta, message);
			Assert.AreEqual (left.Width, right.Width, delta, message);
			Assert.AreEqual (left.Height, right.Height, delta, message);
		}

		[Test]
		public void FloatingPointStretTrampolineTest ()
		{
			RectangleF rect, rect2, rect3, rect4;
#if !__TVOS__
			MKCoordinateRegion mkregion;
			MKMapRect mkmaprect;
#endif
			FloatingPointStretTrampolines obj = new FloatingPointStretTrampolines ();
			IntPtr class_ptr = Class.GetHandle ("FloatingPointStretTrampolines");
			NSString tmp_obj = obj.StringObj;
#if !__WATCHOS__
			Matrix3 matrix3;
			Matrix4 matrix4;
			CATransform3D catransform3d;
#endif // !__WATCHOS__
			int i;
			
			rect2 = new RectangleF (1.2f, 2.3f, 3.4f, 4.5f);
			if (IsArm64) {
				rect = RectangleF_objc_msgSend_IntPtr_IntPtr_RectangleF (obj.Handle, new Selector ("testRectangleF_string_string_RectangleF:b:c:").Handle, new NSString ("a").Handle, new NSString ("b").Handle, rect2);
			} else {
				RectangleF_objc_msgSend_stret_IntPtr_IntPtr_RectangleF (out rect, obj.Handle, new Selector ("testRectangleF_string_string_RectangleF:b:c:").Handle, new NSString ("a").Handle, new NSString ("b").Handle, rect2);
			}
			Assert.That (rect == new RectangleF (rect2.X * pi, rect2.Y * pi, rect2.Width * pi, rect2.Height * pi), "#testRectangleF_string_string_RectangleF:b:c:");

			if (IsArm64) {
				rect = Messaging.RectangleF_objc_msgSend (obj.Handle, new Selector ("testRectangleF").Handle);
			} else {
				Messaging.RectangleF_objc_msgSend_stret (out rect, obj.Handle, new Selector ("testRectangleF").Handle);
			}
			Assert.That (rect == new RectangleF (1.2f, 2.3f, 3.4f, 4.5f), "#testRectangleF");

			if (IsArm64) {
				rect = Messaging.RectangleF_objc_msgSend (class_ptr, new Selector ("staticTestRectangleF").Handle);
			} else {
				Messaging.RectangleF_objc_msgSend_stret (out rect, class_ptr, new Selector ("staticTestRectangleF").Handle);
			}
			Assert.That (rect == new RectangleF (1.2f, 2.3f, 3.4f, 4.5f), "#static testRectangleF");
			
			i = 152;
			if (IsArm64) {
				rect = Messaging.RectangleF_objc_msgSend_int (obj.Handle, new Selector ("testRectangleF_int:").Handle, 152);
			} else {
				Messaging.RectangleF_objc_msgSend_stret_int (out rect, obj.Handle, new Selector ("testRectangleF_int:").Handle, 152);
			}
			Assert.That (rect == new RectangleF (i + .1f, i + .2f, i + .3f, i + .4f), "#ret RectF-int", "#testRectangleF_int:");

			if (IsArm64) {
				rect = Messaging.RectangleF_objc_msgSend_IntPtr (obj.Handle, new Selector ("testRectangleF_IntPtr:").Handle, tmp_obj.Handle);
			} else {
				Messaging.RectangleF_objc_msgSend_stret_IntPtr (out rect, obj.Handle, new Selector ("testRectangleF_IntPtr:").Handle, tmp_obj.Handle);
			}
			AreAlmostEqual (rect, new RectangleF (pi + 0.4f, pi + 0.3f, pi + 0.2f, pi + 0.1f), "#ret RectF-IntPtr");

#if !__TVOS__
			mkregion = new MKCoordinateRegion (new CLLocationCoordinate2D (123.456, 345.678), new MKCoordinateSpan (987.654, 654.321));
			if (IsArm64) {
				rect = Messaging.RectangleF_objc_msgSend_MKCoordinateRegion_IntPtr (obj.Handle, new Selector ("testRectangleF_MCCoordinateRegion_IntPtr:str:").Handle, mkregion, tmp_obj.Handle);
			} else {
				Messaging.RectangleF_objc_msgSend_stret_MKCoordinateRegion_IntPtr (out rect, obj.Handle, new Selector ("testRectangleF_MCCoordinateRegion_IntPtr:str:").Handle, mkregion, tmp_obj.Handle);
			}
			Assert.That (rect == new RectangleF (123.456f+pi, 345.678f-pi, 987.654f*pi, 654.321f/pi), "#testRectangleF_MCCoordinateRegion_IntPtr:str:");
			
			mkmaprect = new MKMapRect (111.1, 222.2, 333.3, 444.4);
			if (IsArm64) {
				rect = Messaging.RectangleF_objc_msgSend_MKMapRect (obj.Handle, new Selector ("testRectangleF_MKMapRect:").Handle, mkmaprect);
			} else {
				Messaging.RectangleF_objc_msgSend_stret_MKMapRect (out rect, obj.Handle, new Selector ("testRectangleF_MKMapRect:").Handle, mkmaprect);
			}
			Assert.That (rect == new RectangleF (111.1f, 222.2f, 333.3f, 444.4f), "#testRectangleF_MKMapRect:");
#endif // !__TVOS__
			
			rect2 = new RectangleF (9.9f, 8.8f, 7.7f, 6.6f);
			if (IsArm64) {
				rect = Messaging.RectangleF_objc_msgSend_RectangleF (obj.Handle, new Selector ("testRectangleF_RectangleF:").Handle, rect2);
			} else {
				Messaging.RectangleF_objc_msgSend_stret_RectangleF (out rect, obj.Handle, new Selector ("testRectangleF_RectangleF:").Handle, rect2);
			}
			Assert.That (rect == rect2, "#testRectangleF_RectangleF:");
			
			rect2 = new RectangleF (5.44f, 4.33f, 3.22f, 2.11f);
			i = 8;
			if (IsArm64) {
				rect = Messaging.RectangleF_objc_msgSend_RectangleF_int (obj.Handle, new Selector ("testRectangleF_RectangleF_int:i:").Handle, rect2, 8);
			} else {
				Messaging.RectangleF_objc_msgSend_stret_RectangleF_int (out rect, obj.Handle, new Selector ("testRectangleF_RectangleF_int:i:").Handle, rect2, 8);
			}
			AreAlmostEqual (rect, new RectangleF (5.44f*i, 4.33f+i, 3.22f-i, 2.11f/i), "testRectangleF_RectangleF_int:i:");
			
			rect2 = new RectangleF (5.4f, 4.3f, 3.2f, 2.1f);
			if (IsArm64) {
				rect = Messaging.RectangleF_objc_msgSend_RectangleF_IntPtr (obj.Handle, new Selector ("testRectangleF_RectangleF_IntPtr:str:").Handle, rect2, tmp_obj.Handle);
			} else {
				Messaging.RectangleF_objc_msgSend_stret_RectangleF_IntPtr (out rect, obj.Handle, new Selector ("testRectangleF_RectangleF_IntPtr:str:").Handle, rect2, tmp_obj.Handle);
			}
			Assert.That (rect == new RectangleF (5.4f*pi, 4.3f+pi, 3.2f-pi, 2.1f/pi));
			
			rect2 = new RectangleF (1, 2, 3, 4);
			rect3 = new RectangleF (9, 8, 7, 6);
			if (IsArm64) {
				rect = Messaging.RectangleF_objc_msgSend_RectangleF_RectangleF_float ( obj.Handle, new Selector ("testRectangleF_RectangleF_RectangleF_float:b:c:").Handle, rect2, rect3, (float) pi);
			} else {
				Messaging.RectangleF_objc_msgSend_stret_RectangleF_RectangleF_float (out rect, obj.Handle, new Selector ("testRectangleF_RectangleF_RectangleF_float:b:c:").Handle, rect2, rect3, (float) pi);
			}
			Assert.That (rect == new RectangleF (1 * 9 * pi, 2 * 8 * pi, 3 * 7 * pi, 4 * 6 * pi), "#testRectangleF_RectangleF_RectangleF_float:b:c:");

			rect2 = new RectangleF (1, 2, 3, 4);
			rect3 = new RectangleF (9, 8, 7, 6);
			rect4 = new RectangleF (10, 20, 30, 40);
			if (IsArm64) {
				rect = Messaging.RectangleF_objc_msgSend_RectangleF_RectangleF_RectangleF (obj.Handle, new Selector ("testRectangleF_RectangleF_RectangleF_RectangleF:b:c:").Handle, rect2, rect3, rect4);
			} else {
				Messaging.RectangleF_objc_msgSend_stret_RectangleF_RectangleF_RectangleF (out rect, obj.Handle, new Selector ("testRectangleF_RectangleF_RectangleF_RectangleF:b:c:").Handle, rect2, rect3, rect4);
			}
			Assert.That (rect == new RectangleF (20, 30, 40, 50), "#testRectangleF_RectangleF_RectangleF_RectangleF:b:c:");

#if !__WATCHOS__
			if (IsArm64) {
				matrix3 = Messaging.Matrix3_objc_msgSend (obj.Handle, new Selector ("testMatrix3").Handle);
			} else {
				Messaging.Matrix3_objc_msgSend_stret (out matrix3, obj.Handle, new Selector ("testMatrix3").Handle);
			}
			Assert.That (matrix3.Equals (new Matrix3 (1, 2, 3, 4, 5, 6, 7, 8, 9)), "#testMatrix3");

			if (IsArm64) {
				matrix4 = Matrix4_objc_msgSend (obj.Handle, new Selector ("testMatrix4").Handle);
			} else {
				Matrix4_objc_msgSend_stret (out matrix4, obj.Handle, new Selector ("testMatrix4").Handle);
			}
			Assert.That (matrix4.Equals (new Matrix4 (9, 8, 7, 6, 5, 4, 3, 2, 1, 9, 8, 7, 6, 5, 4, 3)), "#testMatrix4");

			if (IsArm64) {
				catransform3d = Messaging.CATransform3D_objc_msgSend (obj.Handle, new Selector ("testCATransform3D").Handle);
			} else {
				Messaging.CATransform3D_objc_msgSend_stret (out catransform3d, obj.Handle, new Selector ("testCATransform3D").Handle);
			}
			CATransform3D res = new CATransform3D ();
			res.m11 = 11.1f;
			res.m22 = 22.2f;
			res.m33 = 33.3f;
			res.m44 = 44.4f;
			Assert.That (catransform3d.Equals (res), "#testCATransform3D");
#endif // !__WATCHOS__
			
			PointF point;
			SizeF size;
			
			if (IsArm32) {
				Messaging.PointF_objc_msgSend_stret (out point, obj.Handle, new Selector ("testPointF").Handle);
			} else {
				point = Messaging.PointF_objc_msgSend (obj.Handle, new Selector ("testPointF").Handle);
			}
			Assert.That (point == new PointF (pi*2, pi*20), "#testPointF");
			
			if (IsArm32) {
				Messaging.SizeF_objc_msgSend_stret (out size, obj.Handle, new Selector ("testSizeF").Handle);
			} else {
				size = Messaging.SizeF_objc_msgSend (obj.Handle, new Selector ("testSizeF").Handle);
			}
			Assert.That (size == new SizeF (pi*3, pi*30), "#testSizeF");
		}

		[Test]
		public void OutValueTypeTest ()
		{
			var obj = new OutParams ();
			PointF point = new PointF (3, 14);
			bool res;
			
			res = bool_objc_msgSend_out_PointF (obj.Handle, new Selector ("Test_PointF:").Handle, out point);
			Assert.That (res, "#res");
			Assert.That (point.X, Is.EqualTo ((nfloat) 3.1415f), "#x");
			Assert.That (point.Y, Is.EqualTo ((nfloat) 0), "#y");
		}
		
		[Test]
		public void OutParamTest ()
		{
			var obj = new OutParams ();
			NSError error = new NSError (new NSString ("doomed"), 314);
			IntPtr errorptr = error.Handle;
			
			void_objc_msgSend_out_NSError (obj.Handle, new Selector ("Test_NSError:arg1:").Handle, out errorptr, 0);
			error = errorptr == IntPtr.Zero ? null : Runtime.GetNSObject<NSError> (errorptr);
			Assert.That (error, Is.Null, "#1");
			
			void_objc_msgSend_out_NSError (obj.Handle, new Selector ("Test_NSError:arg1:").Handle, out errorptr, 1);
			error = Runtime.GetNSObject<NSError> (errorptr);
			Assert.That (error.Domain, Is.EqualTo ("domain"), "#2 - domain");
			Assert.That (error.Code, Is.EqualTo ((nint) 1), "#3 - code");
		}
		
		[Test]
		public void ArrayTest ()
		{
			var obj = new ArrayTrampolines ();
			string[] arr = new string [] { "abc" };
			int c;
			
			c = Messaging.int_objc_msgSend_IntPtr (obj.Handle, new Selector ("Test_StringArray:").Handle, NSArray.FromStrings (arr).Handle);
			
			Assert.That (c == 1, "#a1");
			Assert.That (arr [0] == "abc"); // array elements aren't copied back out (maybe they should be?)
			
			arr = NSArray.StringArrayFromHandle (Messaging.IntPtr_objc_msgSend (obj.Handle, new Selector ("Test_StringArrayReturn").Handle));

			Assert.That (arr.Length == 1, "#b1");
			Assert.That (arr [0] == "def", "#b2");
			
			arr = NSArray.StringArrayFromHandle (Messaging.IntPtr_objc_msgSend (obj.Handle, new Selector ("Test_StringArrayNullReturn").Handle));
			Assert.IsNull (arr, "#c1");

			c = Messaging.int_objc_msgSend_IntPtr (obj.Handle, new Selector ("Test_StringArray:").Handle, IntPtr.Zero);
			Assert.AreEqual (-1, c, "#d1");
		}
		
		[Test]
		public void IntPtrTest ()
		{
			var obj = new IntPtrTrampolines ();
			bool res;
			
			res = Messaging.bool_objc_msgSend_IntPtr (obj.Handle, new Selector ("IsZero:").Handle, IntPtr.Zero);
			Assert.That (res, "#1");
			res = Messaging.bool_objc_msgSend_IntPtr (obj.Handle, new Selector ("IsZero:").Handle, new IntPtr (1));
			Assert.That (!res, "#2");
		}

		[Test]
		public void X64ArgumentOverflow ()
		{
			using (var obj = new MiscTrampolines ()) {
				Messaging.void_objc_msgSend_IntPtr_IntPtr_IntPtr_NSRange_IntPtr (obj.Handle, Selector.GetHandle ("x64argumentoverflow:::::"), new IntPtr (1), new IntPtr (2), new IntPtr (3), new NSRange (4, 5), new IntPtr (6));
			}
		}
	}

	[Preserve (AllMembers = true)]
	[Register ("MiscTrampolines")]
	public class MiscTrampolines : NSObject
	{
		[Export ("x64argumentoverflow:::::")]
		void X64ArgumentOverflow (nint a, nint b, nint c, NSRange overflow, nint d)
		{
			Assert.AreEqual ((nint) 1, a, "1");
			Assert.AreEqual ((nint) 2, b, "2");
			Assert.AreEqual ((nint) 3, c, "3");
			Assert.AreEqual ((nint) 4, overflow.Location, "length");
			Assert.AreEqual ((nint) 5, overflow.Length, "location");
			Assert.AreEqual ((nint) 6, d, "4");
		}
	}

	[Register ("IntPtrTrampolines")]
	[Preserve (AllMembers = true)]
	public class IntPtrTrampolines : NSObject
	{
		[Export ("IsZero:")]
		bool IsZero (IntPtr foo)
		{
			return foo == IntPtr.Zero;
		}
	}
	
	[Register ("ArrayTrampolines")]
	[Preserve (AllMembers = true)]
	public class ArrayTrampolines : NSObject
	{
		[Export ("Test_StringArray:")]
		int Test_StringArray (string[] arr)
		{
			if (arr == null)
				return -1;
			if (arr.Length == 0)
				return 0;
			arr [0] = "def";
			return arr.Length;
		}
		
		[Export ("Test_StringArrayReturn")]
		string[] Test_StringArrayReturn ()
		{
			return new string [] { "def" };
		}

		[Export ("Test_StringArrayNullReturn")]
		string[] Test_StringArrayNullReturn ()
		{
			return null;
		}
	}
	
	[Register ("OutParams")]
	[Preserve (AllMembers = true)]
	public class OutParams : NSObject
	{
		[Export ("Test_NSError:arg1:")]
		void Test_NSError (out NSError error, int arg1)
		{
			switch (arg1) {
			case 0:
				error = null;
				break;
			case 1:
				error = new NSError (new NSString ("domain"), 1);
				break;
			default:
				throw new ArgumentOutOfRangeException ("arg1");
			}
		}
		[Export ("Test_PointF:")]
		bool Test_PointF (out PointF point)
		{
			point = new PointF (3.1415f, 0);
			return true;
		}
	}
	
	[Preserve (AllMembers = true)]
	public struct IIIIStruct
	{
		public int A, B, C, D;
		
		public IIIIStruct (int a, int b, int c, int d)
		{
			A = a;
			B = b;
			C = c;
			D = d;
		}
		
		public override string ToString ()
		{
			return string.Format ("[{0};{1};{2};{3}]", A, B, C, D);
		}
	}

	[Preserve (AllMembers = true)]
	public struct FFFFStruct
	{
		public float A, B, C, D;

		public FFFFStruct (float a, float b, float c, float d)
		{
			A = a;
			B = b;
			C = c;
			D = d;
		}

		public override string ToString ()
		{
			return string.Format ("[{0};{1};{2};{3}]", A, B, C, D);
		}
	}

	[Preserve (AllMembers = true)]
	public struct FIFIStruct
	{
		public float A;
		public int B;
		public float C;
		public int D;

		public FIFIStruct (float a, int b, float c, int d)
		{
			A = a;
			B = b;
			C = c;
			D = d;
		}

		public override string ToString ()
		{
			return string.Format ("[{0};{1};{2};{3}]", A, B, C, D);
		}
	}

	[Preserve (AllMembers = true)]
	public struct IFIFStruct
	{
		public int A;
		public float B;
		public int C;
		public float D;

		public IFIFStruct (int a, float b, int c, float d)
		{
			A = a;
			B = b;
			C = c;
			D = d;
		}

		public override string ToString ()
		{
			return string.Format ("[{0};{1};{2};{3}]", A, B, C, D);
		}
	}

	[Preserve (AllMembers = true)]
	[Register ("LongTrampolines")]
	public class LongTrampolines : NSObject
	{
		[Export ("ManyLongs:b:c:d:")]
		void ManyLongs (long a, long b, ref long c, ref long d)
		{
			//Console.WriteLine ("a: {0} b: {1} c: {2} d: {3}", a, b, c, d);
			c = a + b;
			d = b - a;
		}
		
		[Export ("VeryManyLongs:b:c:d:")]
		long VeryManyLongs (long a, long b, out long c, out long d)
		{
			c = a + b;
			d = a - b;
			//Console.WriteLine ("a: {0} b: {1} c: {2} d: {3}", a, b, c, d);
			return a + b + c + d;
		}
	}
	
	[Register ("StretTrampolines")]
	[Preserve (AllMembers = true)]
	public class StretTrampolines : NSObject
	{
		/* IIII */

		[Export ("Test_IIIIStruct")]
		IIIIStruct Test_IIIIStruct ()
		{
			return new IIIIStruct (1, 2, 3, 4);
		}
		
		[Export ("Test_StaticIIIIStruct")]
		static IIIIStruct Test_StaticIIIIStruct ()
		{
			return new IIIIStruct (10, 20, 30, 40);
		}
		
		IIIIStruct Test_IIIIStructProperty {
			[Export ("Test_IIIIStructProperty")]
			get { return new IIIIStruct (100, 200, 300, 400); }
		}
				
		static IIIIStruct Test_StaticIIIIStructProperty {
			[Export ("Test_StaticIIIIStructProperty")]
			get { return new IIIIStruct (1000, 2000, 3000, 4000); }
		}
		
		[Export ("Test_IIIIStruct_out_Double:")]
		IIIIStruct Test_IIIIStruct_out_Double (out double foo)
		{
			foo = 3.14;
			return new IIIIStruct (1, 20, 300, 4000);
		}
		
		[Export ("Test_StaticIIIIStruct_out_Float:")]
		static IIIIStruct Test_StaticIIIIStruct_out_Float (out float foo)
		{
			foo = 3.15f;
			return new IIIIStruct (10, 20, 300, 4000);
		}

		/* FFFF */

		[Export ("Test_FFFFStruct")]
		FFFFStruct Test_FFFFStruct ()
		{
			return new FFFFStruct (1, 2, 3, 4);
		}

		[Export ("Test_StaticFFFFStruct")]
		static FFFFStruct Test_StaticFFFFStructStruct ()
		{
			return new FFFFStruct (10, 20, 30, 40);
		}

		FFFFStruct Test_FFFFStructtStructProperty {
			[Export ("Test_FFFFStructProperty")]
			get { return new FFFFStruct (100, 200, 300, 400); }
		}

		static FFFFStruct Test_StaticFFFFStructProperty {
			[Export ("Test_StaticFFFFStructProperty")]
			get { return new FFFFStruct (1000, 2000, 3000, 4000); }
		}

		[Export ("Test_FFFFStruct_out_Double:")]
		FFFFStruct Test_FFFFStruct_out_Double (out double foo)
		{
			foo = 3.14;
			return new FFFFStruct (1, 20, 300, 4000);
		}

		[Export ("Test_StaticFFFFStruct_out_Float:")]
		static FFFFStruct Test_StaticFFFFStruct_out_Float (out float foo)
		{
			foo = 3.15f;
			return new FFFFStruct (10, 20, 300, 4000);
		}

		/* FIFI */

		[Export ("Test_FIFIStruct")]
		FIFIStruct Test_FIFIStruct ()
		{
			return new FIFIStruct (1, 2, 3, 4);
		}

		[Export ("Test_StaticFIFIStruct")]
		static FIFIStruct Test_StaticFIFIStructStruct ()
		{
			return new FIFIStruct (10, 20, 30, 40);
		}

		FIFIStruct Test_FIFIStructtStructProperty {
			[Export ("Test_FIFIStructProperty")]
			get { return new FIFIStruct (100, 200, 300, 400); }
		}

		static FIFIStruct Test_StaticFIFIStructProperty {
			[Export ("Test_StaticFIFIStructProperty")]
			get { return new FIFIStruct (1000, 2000, 3000, 4000); }
		}

		[Export ("Test_FIFIStruct_out_Double:")]
		FIFIStruct Test_FIFIStruct_out_Double (out double foo)
		{
			foo = 3.14;
			return new FIFIStruct (1, 20, 300, 4000);
		}

		[Export ("Test_StaticFIFIStruct_out_Float:")]
		static FIFIStruct Test_StaticFIFIStruct_out_Float (out float foo)
		{
			foo = 3.15f;
			return new FIFIStruct (10, 20, 300, 4000);
		}

		/* IFIF */

		[Export ("Test_IFIFStruct")]
		IFIFStruct Test_IFIFStruct ()
		{
			return new IFIFStruct (1, 2, 3, 4);
		}

		[Export ("Test_StaticIFIFStruct")]
		static IFIFStruct Test_StaticIFIFStructStruct ()
		{
			return new IFIFStruct (10, 20, 30, 40);
		}

		IFIFStruct Test_IFIFStructtStructProperty {
			[Export ("Test_IFIFStructProperty")]
			get { return new IFIFStruct (100, 200, 300, 400); }
		}

		static IFIFStruct Test_StaticIFIFStructProperty {
			[Export ("Test_StaticIFIFStructProperty")]
			get { return new IFIFStruct (1000, 2000, 3000, 4000); }
		}

		[Export ("Test_IFIFStruct_out_Double:")]
		IFIFStruct Test_IFIFStruct_out_Double (out double foo)
		{
			foo = 3.14;
			return new IFIFStruct (1, 20, 300, 4000);
		}

		[Export ("Test_StaticIFIFStruct_out_Float:")]
		static IFIFStruct Test_StaticIFIFStruct_out_Float (out float foo)
		{
			foo = 3.15f;
			return new IFIFStruct (10, 20, 300, 4000);
		}

#if !__WATCHOS__
		[Export ("myTimeRange")]
		CMTimeRange TimeRange {
			get {
				var rv = new CMTimeRange () { Duration = new CMTime (12, 1), Start = new CMTime (1, 1) };
				return rv;
			}
		}
#endif // !__WATCHOS__
	}
	
	
	[Register ("DoubleStretTrampolines")]
	[Preserve (AllMembers = true)]
	public class DoubleStretTrampolines : NSObject
	{
		[Export ("Test_Zero")]
		double Test_Zero ()
		{
			return 0;
		}
		
		[Export ("Test_StaticZero")]
		static double Test_StaticZero ()
		{
			return 0;
		}
		
		double Test_ZeroProperty {
			[Export ("Test_ZeroProperty")]
			get { return 0; }
		}
				
		static double Test_StaticZeroProperty {
			[Export ("Test_StaticZeroProperty")]
			get { return 0; }
		}
		
		[Export ("Test_Double_out_Double:")]
		double Test_Double_out_Double (out double foo)
		{
			foo = 3.16f;
			return 3.16;
		}
		
		[Export ("Test_StaticDouble_out_Float:")]
		static double Test_StaticDouble_out_Float (out float foo)
		{
			foo = 3.17f;
			return 3.17;
		}
	}
	
	[Register ("FloatStretTrampolines")]
	[Preserve (AllMembers = true)]
	public class FloatStretTrampolines : NSObject
	{
		[Export ("Test_Zero")]
		float Test_Zero ()
		{
			return 0;
		}
		
		[Export ("Test_StaticZero")]
		static float Test_StaticZero ()
		{
			return 0;
		}
		
		float Test_ZeroProperty {
			[Export ("Test_ZeroProperty")]
			get { return 0; }
		}
				
		static float Test_StaticZeroProperty {
			[Export ("Test_StaticZeroProperty")]
			get { return 0; }
		}
		
		[Export ("Test_Float_out_Double:")]
		float Test_Float_out_Double (out double foo)
		{
			foo = 3.18f;
			return 3.18f;
		}
		
		[Export ("Test_StaticFloat_out_Float:")]
		static float Test_StaticFloat_out_Float (out float foo)
		{
			foo = 3.19f;
			return 3.19f;
		}
	}
	
	
	[Register ("FloatingPointStretTrampolines")]
	[Preserve (AllMembers = true)]
	public class FloatingPointStretTrampolines : NSObject
	{
		const float pi = 3.14159f;
		public NSString StringObj = new NSString ("3.14159");
		
		static float ParseString (string str)
		{
			return float.Parse (str, new CultureInfo ("en-US").NumberFormat);
		}
		
		[Export ("testRectangleF")]
		public RectangleF Test_RectangleF ()
		{
			return new RectangleF (1.2f, 2.3f, 3.4f, 4.5f);
		}
		
		[Export ("staticTestRectangleF")]
		static public RectangleF StaticTest_RectangleF ()
		{
			return new RectangleF (1.2f, 2.3f, 3.4f, 4.5f);
		}
		
		[Export ("testRectangleF_int:")]
		public RectangleF Test_RectangleF_int (int a)
		{
			return new RectangleF (a + 0.1f, a + 0.2f, a + 0.3f, a + 0.4f);
		}
		
		[Export ("testRectangleF_IntPtr:")]
		public RectangleF Test_RectangleF_IntPtr (NSString str)
		{
			float pi = ParseString (str.ToString ());
			if ((object) StringObj != (object) str)
				return RectangleF.Empty;
			return new RectangleF (pi + 0.4f, pi + 0.3f, pi + 0.2f, pi + 0.1f);
		}
		
#if !__TVOS__
		[Export ("testRectangleF_MCCoordinateRegion_IntPtr:str:")]
		public RectangleF Test_RectangleF_MKCoordinateRegion_IntPtr (MKCoordinateRegion a, NSString str)
		{
			float pi = ParseString (str.ToString ());
			if ((object) StringObj != (object) str)
				return RectangleF.Empty;
#if __UNIFIED__
			return new RectangleF ((double)(float)a.Center.Latitude+pi, (double)(float)a.Center.Longitude-pi, (double)(float)a.Span.LatitudeDelta*pi, (double)(float)a.Span.LongitudeDelta/pi);
#else
			return new RectangleF ((float)a.Center.Latitude+pi, (float)a.Center.Longitude-pi, (float)a.Span.LatitudeDelta*pi, (float)a.Span.LongitudeDelta/pi);
#endif
		}
		
		[Export ("testRectangleF_MKMapRect:")]
		public RectangleF Test_RectangleF_MKMapRect (MKMapRect a)
		{
			return new RectangleF ((float) a.Origin.X, (float) a.Origin.Y, (float) a.Width, (float) a.Height);
		}
#endif // !__TVOS__
		
		[Export ("testRectangleF_RectangleF:")]
		public RectangleF Test_RectangleF_RectangleF (RectangleF a)
		{
			return new RectangleF (a.X, a.Y, a.Width, a.Height);
		}
		
		[Export ("testRectangleF_RectangleF_int:i:")]
		public RectangleF Test_RectangleF_RectangleF_int (RectangleF a, int i)
		{
			return new RectangleF (a.X*i, a.Y+i, a.Width-i, a.Height/i);
		}
		
		[Export ("testRectangleF_RectangleF_IntPtr:str:")]
		public RectangleF Test_RectangleF_RectangleF_IntPtr (RectangleF a, NSString str)
		{
			float pi = ParseString (str.ToString ());
			if ((object) StringObj != (object) str)
				return RectangleF.Empty;
			return new RectangleF (a.X*pi, a.Y+pi, a.Width-pi, a.Height/pi);
		}
		
		[Export ("testRectangleF_RectangleF_RectangleF_float:b:c:")]
		public RectangleF Test_RectangleF_RectangleF_RectangleF_float (RectangleF a, RectangleF b, float c)
		{
			return new RectangleF (a.X*b.X*c, a.Y*b.Y*c, a.Width*b.Width*c, a.Height*b.Height*c);
		}


		[Export ("testRectangleF_RectangleF_RectangleF_RectangleF:b:c:")]
		public RectangleF Test_RectangleF_RectangleF_RectangleF_RectangleF (RectangleF a, RectangleF b, RectangleF c)
		{
			return new RectangleF (a.X+b.X+c.X, a.Y+b.Y+c.Y, a.Width+b.Width+c.Width, a.Height+b.Height+c.Height);
		}

		[Export ("testRectangleF_string_string_RectangleF:b:c:")]
		public RectangleF Test_RectangleF_string_string_RectangleF (NSString a, NSString b, RectangleF c)
		{
			Assert.That (Is.Equals (a.ToString (), "a"), "#a");
			Assert.That (Is.Equals (b.ToString (), "b"), "#b");
			return new RectangleF (c.X * pi, c.Y * pi, c.Width * pi, c.Height * pi);
		}

#if !__WATCHOS__
		[Export ("testMatrix3")]
		public Matrix3 Test_Matrix3 ()
		{
			return new Matrix3 (1, 2, 3, 4, 5, 6, 7, 8, 9);
		}
		
		[Export ("testMatrix4")]
		public Matrix4 Test_Matrix4 ()
		{
			return new Matrix4 (9, 8, 7, 6, 5, 4, 3, 2, 1, 9, 8, 7, 6, 5, 4, 3);
		}
		
		[Export ("testCATransform3D")]
		public CATransform3D Test_CATransform3D ()
		{
			CATransform3D res = new CATransform3D ();
			res.m11 = 11.1f;
			res.m22 = 22.2f;
			res.m33 = 33.3f;
			res.m44 = 44.4f;
			return res;
		}
#endif // !__WATCHOS__
		
		[Export ("testPointF")]
		public PointF Test_PointF ()
		{
			return new PointF (pi*2, pi*20);
		}

		[Export ("testSizeF")]
		public SizeF Test_SizeF ()
		{
			return new SizeF (pi*3, pi*30);
		}
		
		[Export ("Test_PointF_out_Double:")]
		PointF Test_PointF_out_Double (out double foo)
		{
			foo = 3.18f;
			return new PointF (1, 2);
		}
		
		[Export ("Test_StaticPointF_out_Float:")]
		static PointF Test_StaticPointF_out_Float (out float foo)
		{
			foo = 3.20f;
			return new PointF (10, 20);
		}
	}
}