using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Runtime.InteropServices;

namespace GameCore
{
	/// <summary>
	/// Represents 4-Dimentional vector of single-precision floating point numbers.
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	[StructLayout(LayoutKind.Sequential)]
	public struct FVector4 : ICloneable
	{
		#region Private fields
		internal float mX;
		internal float mY;
		internal float mZ;
		internal float mW;
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="FVector4"/> class with the specified coordinates.
		/// </summary>
		/// <param name="x">The vector's X coordinate.</param>
		/// <param name="y">The vector's Y coordinate.</param>
		/// <param name="z">The vector's Z coordinate.</param>
		/// <param name="w">The vector's W coordinate.</param>
		public FVector4(float x, float y, float z, float w)
		{
			mX = x;
			mY = y;
			mZ = z;
			mW = w;
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="FVector4"/> class with the specified coordinates.
		/// </summary>
		/// <param name="coordinates">An array containing the coordinate parameters.</param>
		public FVector4(float[] coordinates)
		{
			Debug.Assert(coordinates != null);
			Debug.Assert(coordinates.Length >= 4);

			mX = coordinates[0];
			mY = coordinates[1];
			mZ = coordinates[2];
			mW = coordinates[3];
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="FVector4"/> class with the specified coordinates.
		/// </summary>
		/// <param name="coordinates">An array containing the coordinate parameters.</param>
		public FVector4(List<float> coordinates)
		{
			Debug.Assert(coordinates != null);
			Debug.Assert(coordinates.Count >= 4);

			mX = coordinates[0];
			mY = coordinates[1];
			mZ = coordinates[2];
			mW = coordinates[3];
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="FVector4"/> class using coordinates from a given <see cref="FVector4"/> instance.
		/// </summary>
		/// <param name="vector">A <see cref="FVector4"/> to get the coordinates from.</param>
		public FVector4(FVector4 vector)
		{
			mX = vector.X;
			mY = vector.Y;
			mZ = vector.Z;
			mW = vector.W;
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="FVector4"/> class with serialized data.
		/// </summary>
		/// <param name="info">The object that holds the serialized object data.</param>
		/// <param name="context">The contextual information about the source or destination.</param>
		private FVector4(SerializationInfo info, StreamingContext context)
		{
			mX = info.GetSingle("X");
			mY = info.GetSingle("Y");
			mZ = info.GetSingle("Z");
			mW = info.GetSingle("W");
		}
		#endregion

		#region Constants
		/// <summary>
		/// 4-Dimentional single-precision floating point zero vector.
		/// </summary>
		public static readonly FVector4 sZero	= new FVector4(0.0f, 0.0f, 0.0f, 0.0f);
		/// <summary>
		/// 4-Dimentional single-precision floating point X-Axis vector.
		/// </summary>
		public static readonly FVector4 sAxisX	= new FVector4(1.0f, 0.0f, 0.0f, 0.0f);
		/// <summary>
		/// 4-Dimentional single-precision floating point Y-Axis vector.
		/// </summary>
		public static readonly FVector4 sAxisY	= new FVector4(0.0f, 1.0f, 0.0f, 0.0f);
		/// <summary>
		/// 4-Dimentional single-precision floating point Y-Axis vector.
		/// </summary>
		public static readonly FVector4 sAxisZ	= new FVector4(0.0f, 0.0f, 1.0f, 0.0f);
		/// <summary>
		/// 4-Dimentional single-precision floating point Y-Axis vector.
		/// </summary>
		public static readonly FVector4 sAxisW	= new FVector4(0.0f, 0.0f, 0.0f, 1.0f);
		#endregion

		#region Public properties
		/// <summary>
		/// Gets or sets the x-coordinate of this vector.
		/// </summary>
		/// <value>The x-coordinate of this vector.</value>
		public float X
		{
			get { return mX; }
			set { mX = value;}
		}
		/// <summary>
		/// Gets or sets the y-coordinate of this vector.
		/// </summary>
		/// <value>The y-coordinate of this vector.</value>
		public float Y
		{
			get { return mY; }
			set { mY = value;}
		}
		/// <summary>
		/// Gets or sets the z-coordinate of this vector.
		/// </summary>
		/// <value>The z-coordinate of this vector.</value>
		public float Z
		{
			get { return mZ; }
			set { mZ = value;}
		}
		/// <summary>
		/// Gets or sets the w-coordinate of this vector.
		/// </summary>
		/// <value>The w-coordinate of this vector.</value>
		public float W
		{
			get { return mW; }
			set { mW = value;}
		}		
		#endregion

		#region ICloneable Members
		/// <summary>
		/// Creates an exact copy of this <see cref="FVector4"/> object.
		/// </summary>
		/// <returns>The <see cref="FVector4"/> object this method creates, cast as an object.</returns>
		object ICloneable.Clone()
		{
			return new FVector4(this);
		}
		/// <summary>
		/// Creates an exact copy of this <see cref="FVector4"/> object.
		/// </summary>
		/// <returns>The <see cref="FVector4"/> object this method creates.</returns>
		public FVector4 Clone()
		{
			return new FVector4(this);
		}
		#endregion

		#region Public Static Parse Methods
		/// <summary>
		/// Converts the specified string to its <see cref="FVector4"/> equivalent.
		/// </summary>
		/// <param name="s">A string representation of a <see cref="FVector4"/></param>
		/// <returns>A <see cref="FVector4"/> that represents the vector specified by the <paramref name="s"/> parameter.</returns>
		public static FVector4 Parse(string s)
		{
			Regex r = new Regex(@"\((?<x>.*),(?<y>.*),(?<z>.*),(?<w>.*)\)", RegexOptions.None);
			Match m = r.Match(s);
			if (m.Success)
			{
				return new FVector4(
					float.Parse(m.Result("${x}")),
					float.Parse(m.Result("${y}")),
					float.Parse(m.Result("${z}")),
					float.Parse(m.Result("${w}"))
					);
			}
			else
			{
				throw new ParseException("Unsuccessful Match.");
			}
		}
		#endregion

		#region Public Static Vector Arithmetics
		/// <summary>
		/// Adds two vectors.
		/// </summary>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <param name="w">A <see cref="FVector4"/> instance.</param>
		/// <returns>A new <see cref="FVector4"/> instance containing the sum.</returns>
		public static FVector4 Add(FVector4 v, FVector4 w)
		{
			return new FVector4(v.X + w.X, v.Y + w.Y, v.Z + w.Z, v.W + w.W);
		}
		/// <summary>
		/// Adds a vector and a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector4"/> instance containing the sum.</returns>
		public static FVector4 Add(FVector4 v, float s)
		{
			return new FVector4(v.X + s, v.Y + s, v.Z + s, v.W +s);
		}
		/// <summary>
		/// Adds two vectors and put the result in the third vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector4"/> instance.</param>
		/// <param name="v">A <see cref="FVector4"/> instance</param>
		/// <param name="w">A <see cref="FVector4"/> instance to hold the result.</param>
		public static void Add(FVector4 u, FVector4 v, FVector4 w)
		{
			w.X = u.X + v.X;
			w.Y = u.Y + v.Y;
			w.Z = u.Z + v.Z;
			w.W = u.W + v.W;
		}
		/// <summary>
		/// Adds a vector and a scalar and put the result into another vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector4"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <param name="v">A <see cref="FVector4"/> instance to hold the result.</param>
		public static void Add(FVector4 u, float s, FVector4 v)
		{
			v.X = u.X + s;
			v.Y = u.Y + s;
			v.Z = u.Z + s;
			v.W = u.W + s;
		}
		/// <summary>
		/// Subtracts a vector from a vector.
		/// </summary>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <param name="w">A <see cref="FVector4"/> instance.</param>
		/// <returns>A new <see cref="FVector4"/> instance containing the difference.</returns>
		/// <remarks>
		///	result[i] = v[i] - w[i].
		/// </remarks>
		public static FVector4 Subtract(FVector4 v, FVector4 w)
		{
			return new FVector4(v.X - w.X, v.Y - w.Y, v.Z - w.Z, v.W - w.W);
		}
		/// <summary>
		/// Subtracts a scalar from a vector.
		/// </summary>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector4"/> instance containing the difference.</returns>
		/// <remarks>
		/// result[i] = v[i] - s
		/// </remarks>
		public static FVector4 Subtract(FVector4 v, float s)
		{
			return new FVector4(v.X - s, v.Y - s, v.Z - s, v.W - s);
		}
		/// <summary>
		/// Subtracts a vector from a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector4"/> instance containing the difference.</returns>
		/// <remarks>
		/// result[i] = s - v[i]
		/// </remarks>
		public static FVector4 Subtract(float s, FVector4 v)
		{
			return new FVector4(s - v.X, s - v.Y, s - v.Z, s - v.W);
		}
		/// <summary>
		/// Subtracts a vector from a second vector and puts the result into a third vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector4"/> instance.</param>
		/// <param name="v">A <see cref="FVector4"/> instance</param>
		/// <param name="w">A <see cref="FVector4"/> instance to hold the result.</param>
		/// <remarks>
		///	w[i] = v[i] - w[i].
		/// </remarks>
		public static void Subtract(FVector4 u, FVector4 v, FVector4 w)
		{
			w.X = u.X - v.X;
			w.Y = u.Y - v.Y;
			w.Z = u.Z - v.Z;
			w.W = u.W - v.W;
		}
		/// <summary>
		/// Subtracts a vector from a scalar and put the result into another vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector4"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <param name="v">A <see cref="FVector4"/> instance to hold the result.</param>
		/// <remarks>
		/// v[i] = u[i] - s
		/// </remarks>
		public static void Subtract(FVector4 u, float s, FVector4 v)
		{
			v.X = u.X - s;
			v.Y = u.Y - s;
			v.Z = u.Z - s;
			v.W = u.W - s;
		}
		/// <summary>
		/// Subtracts a scalar from a vector and put the result into another vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector4"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <param name="v">A <see cref="FVector4"/> instance to hold the result.</param>
		/// <remarks>
		/// v[i] = s - u[i]
		/// </remarks>
		public static void Subtract(float s, FVector4 u, FVector4 v)
		{
			v.X = s - u.X;
			v.Y = s - u.Y;
			v.Z = s - u.Z;
			v.W = s - u.W;
		}
		/// <summary>
		/// Divides a vector by another vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector4"/> instance.</param>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <returns>A new <see cref="FVector4"/> containing the quotient.</returns>
		/// <remarks>
		///	result[i] = u[i] / v[i].
		/// </remarks>
		public static FVector4 Divide(FVector4 u, FVector4 v)
		{
			return new FVector4(u.X / v.X, u.Y / v.Y, u.Z / v.Z, u.W / v.W);
		}
		/// <summary>
		/// Divides a vector by a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <returns>A new <see cref="FVector4"/> containing the quotient.</returns>
		/// <remarks>
		/// result[i] = v[i] / s;
		/// </remarks>
		public static FVector4 Divide(FVector4 v, float s)
		{
			return new FVector4(v.X / s, v.Y / s, v.Z / s, v.W / s);
		}
		/// <summary>
		/// Divides a scalar by a vector.
		/// </summary>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <returns>A new <see cref="FVector4"/> containing the quotient.</returns>
		/// <remarks>
		/// result[i] = s / v[i]
		/// </remarks>
		public static FVector4 Divide(float s, FVector4 v)
		{
			return new FVector4(s / v.X, s/ v.Y, s / v.Z, s/ v.W);
		}
		/// <summary>
		/// Divides a vector by another vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector4"/> instance.</param>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <param name="w">A <see cref="FVector4"/> instance to hold the result.</param>
		/// <remarks>
		/// w[i] = u[i] / v[i]
		/// </remarks>
		public static void Divide(FVector4 u, FVector4 v, FVector4 w)
		{
			w.X = u.X / v.X;
			w.Y = u.Y / v.Y;
			w.Z = u.Z / v.Z;
			w.W = u.W / v.W;
		}
		/// <summary>
		/// Divides a vector by a scalar.
		/// </summary>
		/// <param name="u">A <see cref="FVector4"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <param name="v">A <see cref="FVector4"/> instance to hold the result.</param>
		/// <remarks>
		/// v[i] = u[i] / s
		/// </remarks>
		public static void Divide(FVector4 u, float s, FVector4 v)
		{
			v.X = u.X / s;
			v.Y = u.Y / s;
			v.Z = u.Z / s;
			v.W = u.W / s;
		}
		/// <summary>
		/// Divides a scalar by a vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector4"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <param name="v">A <see cref="FVector4"/> instance to hold the result.</param>
		/// <remarks>
		/// v[i] = s / u[i]
		/// </remarks>
		public static void Divide(float s, FVector4 u, FVector4 v)
		{
			v.X = s / u.X;
			v.Y = s / u.Y;
			v.Z = s / u.Z;
			v.W = s / u.W;
		}
		/// <summary>
		/// Multiplies a vector by a scalar.
		/// </summary>
		/// <param name="u">A <see cref="FVector4"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector4"/> containing the result.</returns>
		public static FVector4 Multiply(FVector4 u, float s)
		{
			return new FVector4(u.X * s, u.Y * s, u.Z * s, u.W * s);
		}
		/// <summary>
		/// Multiplies a vector by a scalar and put the result in another vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector4"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <param name="v">A <see cref="FVector4"/> instance to hold the result.</param>
		public static void Multiply(FVector4 u, float s, FVector4 v)
		{
			v.X = u.X * s;
			v.Y = u.Y * s;
			v.Z = u.Z * s;
			v.W = u.W * s;
		}
		/// <summary>
		/// Calculates the dot product of two vectors.
		/// </summary>
		/// <param name="u">A <see cref="FVector4"/> instance.</param>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <returns>The dot product value.</returns>
		public static float DotProduct(FVector4 u, FVector4 v)
		{
			return (u.X * v.X) + (u.Y * v.Y) + (u.Z * v.Z) + (u.W * v.W);
		}
		/// <summary>
		/// Calculates the dot product of two vectors.
		/// </summary>
		/// <param name="u">A <see cref="FVector4"/> instance.</param>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <returns>The dot product value.</returns>
		public static FVector4 CrossProduct(FVector4 u, FVector4 v)
		{
			throw new System.NotImplementedException();
		}
		/// <summary>
		/// Negates a vector.
		/// </summary>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <returns>A new <see cref="FVector4"/> instance containing the negated values.</returns>
		public static FVector4 Negate(FVector4 v)
		{
			return new FVector4(-v.X, -v.Y, -v.Z, -v.W);
		}
		/// <summary>
		/// Tests whether two vectors are approximately equal using default tolerance value.
		/// </summary>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <param name="u">A <see cref="FVector4"/> instance.</param>
		/// <returns>True if the two vectors are approximately equal; otherwise, False.</returns>
		public static bool ApproxEqual(FVector4 v, FVector4 u)
		{
			return ApproxEqual(v,u, Math.Epsilon);
		}
		/// <summary>
		/// Tests whether two vectors are approximately equal given a tolerance value.
		/// </summary>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <param name="u">A <see cref="FVector4"/> instance.</param>
		/// <param name="tolerance">The tolerance value used to test approximate equality.</param>
		/// <returns>True if the two vectors are approximately equal; otherwise, False.</returns>
		public static bool ApproxEqual(FVector4 v, FVector4 u, float tolerance)
		{
			return
				(
				(System.Math.Abs(v.X - u.X) <= tolerance) &&
				(System.Math.Abs(v.Y - u.Y) <= tolerance)
				);
		}
		#endregion

		#region Public Methods

		/// <summary>
		/// Interpolate this vector to the incoming vector using inFactor
		/// </summary>
		/// <param name="inTo"></param>
		/// <param name="inFactor"></param>
		public void LinearInterpolate(FVector4 inTo, float inFactor)
		{
			mX = mX + (inTo.mX - mX) * inFactor;
			mY = mY + (inTo.mY - mY) * inFactor;
			mZ = mZ + (inTo.mZ - mZ) * inFactor;
			mW = mW + (inTo.mW - mW) * inFactor;
		}

		/// <summary>
		/// Intitialize from a FVector3 and leaves the w component untouched
		/// </summary>
		/// <param name="inVector"></param>
		public void Set(FVector3 inVector)
		{
			mX = inVector.mX;
			mY = inVector.mY;
			mZ = inVector.mZ;
		}

		/// <summary>
		/// Scale the vector so that its length is 1.
		/// </summary>
		public void Normalize()
		{
			float length = Length();
			if (length == 0)
			{
				throw new DivideByZeroException("Trying to normalize a vector with length of zero.");
			}

			mX /= length;
			mY /= length;
			mZ /= length;
			mW /= length;
		}
		/// <summary>
		/// Check if vector is of length 1
		/// </summary>
		/// <returns></returns>
		public bool IsNormalized(float inEpsilon)
		{
			float l = Length();
			return Math.IsNear(l, 1.0f, inEpsilon);
		}
		/// <summary>
		/// Returns the length of the vector.
		/// </summary>
		/// <returns>The length of the vector. (Sqrt(X*X + Y*Y))</returns>
		public float Length()
		{
			return (float)System.Math.Sqrt(mX * mX + mY * mY + mZ * mZ + mW * mW);
		}
		/// <summary>
		/// Returns the length of the x/y/z part of the vector
		/// </summary>
		/// <returns></returns>
		public float Length3()
		{
			return (float)System.Math.Sqrt(mX * mX + mY * mY + mZ * mZ);
		}
		/// <summary>
		/// Returns the squared length of the vector.
		/// </summary>
		/// <returns>The squared length of the vector. (X*X + Y*Y)</returns>
		public float SquareLength()
		{
			return (mX*mX + mY*mY + mZ*mZ + mW*mW);
		}
		#endregion

		#region Overrides
		/// <summary>
		/// Returns the hashcode for this instance.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			return mX.GetHashCode() ^ mY.GetHashCode() ^ mZ.GetHashCode() ^ mW.GetHashCode();
		}
		/// <summary>
		/// Returns a value indicating whether this instance is equal to
		/// the specified object.
		/// </summary>
		/// <param name="obj">An object to compare to this instance.</param>
		/// <returns>True if <paramref name="obj"/> is a <see cref="FVector4"/> and has the same values as this instance; otherwise, False.</returns>
		public override bool Equals(object obj)
		{
			if (obj is FVector4)
			{
				FVector4 v = (FVector4)obj;
				return (mX == v.X) && (mY == v.Y) && (mZ == v.Z) && (mW == v.W);
			}
			return false;
		}

		/// <summary>
		/// Returns a string representation of this object.
		/// </summary>
		/// <returns>A string representation of this object.</returns>
		public override string ToString()
		{
			return string.Format("({0}, {1}, {2}, {3})", mX, mY, mZ, mW);
		}
		#endregion
		
		#region Comparison Operators
		/// <summary>
		/// Tests whether two specified vectors are equal.
		/// </summary>
		/// <param name="u">The left-hand vector.</param>
		/// <param name="v">The right-hand vector.</param>
		/// <returns>True if the two vectors are equal; otherwise, False.</returns>
		public static bool operator==(FVector4 u, FVector4 v)
		{
			if (Object.Equals(u, null))
			{
				return Object.Equals(v, null);
			}

			if (Object.Equals(v, null))
			{
				return Object.Equals(u, null);
			}

			return (u.X == v.X) && (u.Y == v.Y) && (u.Z == v.Z) && (u.W == v.W);
		}
		/// <summary>
		/// Tests whether two specified vectors are not equal.
		/// </summary>
		/// <param name="u">The left-hand vector.</param>
		/// <param name="v">The right-hand vector.</param>
		/// <returns>True if the two vectors are not equal; otherwise, False.</returns>
		public static bool operator!=(FVector4 u, FVector4 v)
		{
			if (Object.Equals(u, null))
			{
				return !Object.Equals(v, null);
			}

			if (Object.Equals(v, null))
			{
				return !Object.Equals(u, null);
			}

			return !((u.X == v.X) && (u.Y == v.Y) && (u.Z == v.Z) && (u.W == v.W));
		}

		/// <summary>
		/// Tests if a vector's components are greater than another vector's components.
		/// </summary>
		/// <param name="u">The left-hand vector.</param>
		/// <param name="v">The right-hand vector.</param>
		/// <returns>True if the left-hand vector's components are greater than the right-hand vector's component; otherwise, False.</returns>
		public static bool operator>(FVector4 u, FVector4 v)
		{
			return (
				(u.mX > v.mX) && 
				(u.mY > v.mY) && 
				(u.mZ > v.mZ) && 
				(u.mW > v.mW));
		}
		/// <summary>
		/// Tests if a vector's components are smaller than another vector's components.
		/// </summary>
		/// <param name="u">The left-hand vector.</param>
		/// <param name="v">The right-hand vector.</param>
		/// <returns>True if the left-hand vector's components are smaller than the right-hand vector's component; otherwise, False.</returns>
		public static bool operator<(FVector4 u, FVector4 v)
		{
			return (
				(u.mX < v.mX) && 
				(u.mY < v.mY) && 
				(u.mZ < v.mZ) && 
				(u.mW < v.mW));
		}
		/// <summary>
		/// Tests if a vector's components are greater or equal than another vector's components.
		/// </summary>
		/// <param name="u">The left-hand vector.</param>
		/// <param name="v">The right-hand vector.</param>
		/// <returns>True if the left-hand vector's components are greater or equal than the right-hand vector's component; otherwise, False.</returns>
		public static bool operator>=(FVector4 u, FVector4 v)
		{
			return (
				(u.mX >= v.mX) && 
				(u.mY >= v.mY) && 
				(u.mZ >= v.mZ) && 
				(u.mW >= v.mW));
		}
		/// <summary>
		/// Tests if a vector's components are smaller or equal than another vector's components.
		/// </summary>
		/// <param name="u">The left-hand vector.</param>
		/// <param name="v">The right-hand vector.</param>
		/// <returns>True if the left-hand vector's components are smaller or equal than the right-hand vector's component; otherwise, False.</returns>
		public static bool operator<=(FVector4 u, FVector4 v)
		{
			return (
				(u.mX <= v.mX) && 
				(u.mY <= v.mY) && 
				(u.mZ <= v.mZ) && 
				(u.mW <= v.mW));
		}
		#endregion

		#region Unary Operators
		/// <summary>
		/// Negates the values of the vector.
		/// </summary>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <returns>A new <see cref="FVector4"/> instance containing the negated values.</returns>
		public static FVector4 operator-(FVector4 v)
		{
			return FVector4.Negate(v);
		}
		#endregion

		#region Binary Operators
		/// <summary>
		/// Adds two vectors.
		/// </summary>
		/// <param name="u">A <see cref="FVector4"/> instance.</param>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <returns>A new <see cref="FVector4"/> instance containing the sum.</returns>
		public static FVector4 operator+(FVector4 u, FVector4 v)
		{
			return FVector4.Add(u,v);
		}
		/// <summary>
		/// Adds a vector and a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector4"/> instance containing the sum.</returns>
		public static FVector4 operator+(FVector4 v, float s)
		{
			return FVector4.Add(v,s);
		}
		/// <summary>
		/// Adds a vector and a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector4"/> instance containing the sum.</returns>
		public static FVector4 operator+(float s, FVector4 v)
		{
			return FVector4.Add(v,s);
		}
		/// <summary>
		/// Subtracts a vector from a vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector4"/> instance.</param>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <returns>A new <see cref="FVector4"/> instance containing the difference.</returns>
		/// <remarks>
		///	result[i] = v[i] - w[i].
		/// </remarks>
		public static FVector4 operator-(FVector4 u, FVector4 v)
		{
			return FVector4.Subtract(u,v);
		}
		/// <summary>
		/// Subtracts a vector from a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector4"/> instance containing the difference.</returns>
		/// <remarks>
		/// result[i] = v[i] - s
		/// </remarks>
		public static FVector4 operator-(FVector4 v, float s)
		{
			return FVector4.Subtract(v, s);
		}
		/// <summary>
		/// Subtracts a scalar from a vector.
		/// </summary>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector4"/> instance containing the difference.</returns>
		/// <remarks>
		/// result[i] = s - v[i]
		/// </remarks>
		public static FVector4 operator-(float s, FVector4 v)
		{
			return FVector4.Subtract(s, v);
		}

		/// <summary>
		/// Multiplies a vector by a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector4"/> containing the result.</returns>
		public static FVector4 operator*(FVector4 v, float s)
		{
			return FVector4.Multiply(v,s);
		}
		/// <summary>
		/// Multiplies a vector by a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector4"/> containing the result.</returns>
		public static FVector4 operator*(float s, FVector4 v)
		{
			return FVector4.Multiply(v,s);
		}
		/// <summary>
		/// Divides a vector by a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <returns>A new <see cref="FVector4"/> containing the quotient.</returns>
		/// <remarks>
		/// result[i] = v[i] / s;
		/// </remarks>
		public static FVector4 operator/(FVector4 v, float s)
		{
			return FVector4.Divide(v,s);
		}
		/// <summary>
		/// Divides a scalar by a vector.
		/// </summary>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <returns>A new <see cref="FVector4"/> containing the quotient.</returns>
		/// <remarks>
		/// result[i] = s / v[i]
		/// </remarks>
		public static FVector4 operator/(float s, FVector4 v)
		{
			return FVector4.Divide(s,v);
		}
		#endregion

		#region Array Indexing Operator
		/// <summary>
		/// Indexer ( [x, y] ).
		/// </summary>
		public float this[int index]
		{
			get	
			{
				switch( index ) 
				{
					case 0:
						return mX;
					case 1:
						return mY;
					case 2:
						return mZ;
					case 3:
						return mW;
					default:
						throw new IndexOutOfRangeException();
				}
			}
			set 
			{
				switch( index ) 
				{
					case 0:
						mX = value;
						break;
					case 1:
						mY = value;
						break;
					case 2:
						mZ = value;
						break;
					case 3:
						mW = value;
						break;
					default:
						throw new IndexOutOfRangeException();
				}
			}

		}

		#endregion

		#region Conversion Operators
		/// <summary>
		/// Converts the vector to an array of single-precision floating point values.
		/// </summary>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <returns>An array of single-precision floating point values.</returns>
		public static explicit operator float[](FVector4 v)
		{
			float[] array = new float[4];
			array[0] = v.X;
			array[1] = v.Y;
			array[2] = v.Z;
			array[3] = v.W;
			return array;
		}
		/// <summary>
		/// Converts the vector to an array of single-precision floating point values.
		/// </summary>
		/// <param name="v">A <see cref="FVector4"/> instance.</param>
		/// <returns>An array of single-precision floating point values.</returns>
        public static explicit operator List<float>(FVector4 v)
		{
            List<float> array = new List<float>(4);
			array[0] = v.X;
			array[1] = v.Y;
			array[2] = v.Z;
			array[3] = v.W;
			return array;
		}
		#endregion

	}

}
