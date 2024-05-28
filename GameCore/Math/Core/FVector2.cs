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
	/// Represents 2-Dimentional vector of single-precision floating point numbers.
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	[StructLayout(LayoutKind.Sequential)]
	public struct FVector2 : ICloneable
	{
		#region Private fields
		internal float mX;
		internal float mY;
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="FVector2"/> class with the specified coordinates.
		/// </summary>
		/// <param name="x">The vector's X coordinate.</param>
		/// <param name="y">The vector's Y coordinate.</param>
		public FVector2(float x, float y)
		{
			mX = x;
			mY = y;
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="FVector2"/> class with the specified coordinates.
		/// </summary>
		/// <param name="coordinates">An array containing the coordinate parameters.</param>
		public FVector2(float[] coordinates)
		{
			Debug.Assert(coordinates != null);
			Debug.Assert(coordinates.Length >= 2);

			mX = coordinates[0];
			mY = coordinates[1];
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="FVector2"/> class with the specified coordinates.
		/// </summary>
		/// <param name="coordinates">An array containing the coordinate parameters.</param>
        public FVector2(List<float> coordinates)
		{
			Debug.Assert(coordinates != null);
			Debug.Assert(coordinates.Count >= 2);

			mX = coordinates[0];
			mY = coordinates[1];
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="FVector2"/> class using coordinates from a given <see cref="FVector2"/> instance.
		/// </summary>
		/// <param name="vector">A <see cref="FVector2"/> to get the coordinates from.</param>
		public FVector2(FVector2 vector)
		{
			mX = vector.X;
			mY = vector.Y;
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="FVector2"/> class with serialized data.
		/// </summary>
		/// <param name="info">The object that holds the serialized object data.</param>
		/// <param name="context">The contextual information about the source or destination.</param>
		private FVector2(SerializationInfo info, StreamingContext context)
		{
			mX = info.GetSingle("X");
			mY = info.GetSingle("Y");
		}
		#endregion

		#region Constants
		/// <summary>
		/// 2-Dimentional single-precision floating point zero vector.
		/// </summary>
		public static readonly FVector2 sZero	= new FVector2(0.0f, 0.0f);
		/// <summary>
		/// 2-Dimentional single-precision floating point X-Axis vector.
		/// </summary>
		public static readonly FVector2 sAxisX	= new FVector2(1.0f, 0.0f);
		/// <summary>
		/// 2-Dimentional single-precision floating point Y-Axis vector.
		/// </summary>
		public static readonly FVector2 sAxisY	= new FVector2(0.0f, 1.0f);
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
		#endregion

		#region ICloneable Members
		/// <summary>
		/// Creates an exact copy of this <see cref="FVector2"/> object.
		/// </summary>
		/// <returns>The <see cref="FVector2"/> object this method creates, cast as an object.</returns>
		object ICloneable.Clone()
		{
			return new FVector2(this);
		}
		/// <summary>
		/// Creates an exact copy of this <see cref="FVector2"/> object.
		/// </summary>
		/// <returns>The <see cref="FVector2"/> object this method creates.</returns>
		public FVector2 Clone()
		{
			return new FVector2(this);
		}
		#endregion

		#region Public Static Parse Methods
		/// <summary>
		/// Converts the specified string to its <see cref="FVector2"/> equivalent.
		/// </summary>
		/// <param name="s">A string representation of a <see cref="FVector2"/></param>
		/// <returns>A <see cref="FVector2"/> that represents the vector specified by the <paramref name="s"/> parameter.</returns>
		public static FVector2 Parse(string s)
		{
			var r = new Regex(@"\((?<x>.*),(?<y>.*)\)", RegexOptions.None);
			var m = r.Match(s);
			if (m.Success)
			{
				return new FVector2(
					float.Parse(m.Result("${x}")),
					float.Parse(m.Result("${y}"))
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
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <param name="w">A <see cref="FVector2"/> instance.</param>
		/// <returns>A new <see cref="FVector2"/> instance containing the sum.</returns>
		public static FVector2 Add(FVector2 v, FVector2 w)
		{
			return new FVector2(v.X + w.X, v.Y + w.Y);
		}
		/// <summary>
		/// Adds a vector and a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector2"/> instance containing the sum.</returns>
		public static FVector2 Add(FVector2 v, float s)
		{
			return new FVector2(v.X + s, v.Y + s);
		}
		/// <summary>
		/// Adds two vectors and put the result in the third vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector2"/> instance.</param>
		/// <param name="v">A <see cref="FVector2"/> instance</param>
		/// <param name="w">A <see cref="FVector2"/> instance to hold the result.</param>
		public static void Add(FVector2 u, FVector2 v, FVector2 w)
		{
			w.X = u.X + v.X;
			w.Y = u.Y + v.Y;
		}
		/// <summary>
		/// Adds a vector and a scalar and put the result into another vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <param name="v">A <see cref="FVector2"/> instance to hold the result.</param>
		public static void Add(FVector2 u, float s, FVector2 v)
		{
			v.X = u.X + s;
			v.Y = u.Y + s;
		}
		/// <summary>
		/// Subtracts a vector from a vector.
		/// </summary>
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <param name="w">A <see cref="FVector2"/> instance.</param>
		/// <returns>A new <see cref="FVector2"/> instance containing the difference.</returns>
		/// <remarks>
		///	result[i] = v[i] - w[i].
		/// </remarks>
		public static FVector2 Subtract(FVector2 v, FVector2 w)
		{
			return new FVector2(v.X - w.X, v.Y - w.Y);
		}
		/// <summary>
		/// Subtracts a scalar from a vector.
		/// </summary>
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector2"/> instance containing the difference.</returns>
		/// <remarks>
		/// result[i] = v[i] - s
		/// </remarks>
		public static FVector2 Subtract(FVector2 v, float s)
		{
			return new FVector2(v.X - s, v.Y - s);
		}
		/// <summary>
		/// Subtracts a vector from a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector2"/> instance containing the difference.</returns>
		/// <remarks>
		/// result[i] = s - v[i]
		/// </remarks>
		public static FVector2 Subtract(float s, FVector2 v)
		{
			return new FVector2(s - v.X, s - v.Y);
		}
		/// <summary>
		/// Subtracts a vector from a second vector and puts the result into a third vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector2"/> instance.</param>
		/// <param name="v">A <see cref="FVector2"/> instance</param>
		/// <param name="w">A <see cref="FVector2"/> instance to hold the result.</param>
		/// <remarks>
		///	w[i] = v[i] - w[i].
		/// </remarks>
		public static void Subtract(FVector2 u, FVector2 v, FVector2 w)
		{
			w.X = u.X - v.X;
			w.Y = u.Y - v.Y;
		}
		/// <summary>
		/// Subtracts a vector from a scalar and put the result into another vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <param name="v">A <see cref="FVector2"/> instance to hold the result.</param>
		/// <remarks>
		/// v[i] = u[i] - s
		/// </remarks>
		public static void Subtract(FVector2 u, float s, FVector2 v)
		{
			v.X = u.X - s;
			v.Y = u.Y - s;
		}
		/// <summary>
		/// Subtracts a scalar from a vector and put the result into another vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <param name="v">A <see cref="FVector2"/> instance to hold the result.</param>
		/// <remarks>
		/// v[i] = s - u[i]
		/// </remarks>
		public static void Subtract(float s, FVector2 u, FVector2 v)
		{
			v.X = s - u.X;
			v.Y = s - u.Y;
		}
		/// <summary>
		/// Divides a vector by another vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector2"/> instance.</param>
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <returns>A new <see cref="FVector2"/> containing the quotient.</returns>
		/// <remarks>
		///	result[i] = u[i] / v[i].
		/// </remarks>
		public static FVector2 Divide(FVector2 u, FVector2 v)
		{
			return new FVector2(u.X / v.X, u.Y / v.Y);
		}
		/// <summary>
		/// Divides a vector by a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <returns>A new <see cref="FVector2"/> containing the quotient.</returns>
		/// <remarks>
		/// result[i] = v[i] / s;
		/// </remarks>
		public static FVector2 Divide(FVector2 v, float s)
		{
			return new FVector2(v.X / s, v.Y / s);
		}
		/// <summary>
		/// Divides a scalar by a vector.
		/// </summary>
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <returns>A new <see cref="FVector2"/> containing the quotient.</returns>
		/// <remarks>
		/// result[i] = s / v[i]
		/// </remarks>
		public static FVector2 Divide(float s, FVector2 v)
		{
			return new FVector2(s / v.X, s/ v.Y);
		}
		/// <summary>
		/// Divides a vector by another vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector2"/> instance.</param>
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <param name="w">A <see cref="FVector2"/> instance to hold the result.</param>
		/// <remarks>
		/// w[i] = u[i] / v[i]
		/// </remarks>
		public static void Divide(FVector2 u, FVector2 v, FVector2 w)
		{
			w.X = u.X / v.X;
			w.Y = u.Y / v.Y;
		}
		/// <summary>
		/// Divides a vector by a scalar.
		/// </summary>
		/// <param name="u">A <see cref="FVector2"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <param name="v">A <see cref="FVector2"/> instance to hold the result.</param>
		/// <remarks>
		/// v[i] = u[i] / s
		/// </remarks>
		public static void Divide(FVector2 u, float s, FVector2 v)
		{
			v.X = u.X / s;
			v.Y = u.Y / s;
		}
		/// <summary>
		/// Divides a scalar by a vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector2"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <param name="v">A <see cref="FVector2"/> instance to hold the result.</param>
		/// <remarks>
		/// v[i] = s / u[i]
		/// </remarks>
		public static void Divide(float s, FVector2 u, FVector2 v)
		{
			v.X = s / u.X;
			v.Y = s / u.Y;
		}
		/// <summary>
		/// Multiplies a vector with a vector (dot product)
		/// </summary>
		/// <param name="u">A <see cref="FVector2"/> instance.</param>
		/// <param name="s">A <see cref="FVector2"/> instance.</param>
		/// <returns>A new <see cref="FVector2"/> containing the result.</returns>
		public static FVector2 Multiply(FVector2 u, FVector2 v)
		{
			return new FVector2(u.X * v.X, u.Y * v.Y);
		}
		/// <summary>
		/// Multiplies a vector by a scalar.
		/// </summary>
		/// <param name="u">A <see cref="FVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector2"/> containing the result.</returns>
		public static FVector2 Multiply(FVector2 u, float s)
		{
			return new FVector2(u.X * s, u.Y * s);
		}
		/// <summary>
		/// Multiplies a vector by a scalar and put the result in another vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <param name="v">A <see cref="FVector2"/> instance to hold the result.</param>
		public static void Multiply(FVector2 u, float s, FVector2 v)
		{
			v.X = u.X * s;
			v.Y = u.Y * s;
		}
		/// <summary>
		/// Calculates the dot product of two vectors.
		/// </summary>
		/// <param name="u">A <see cref="FVector2"/> instance.</param>
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <returns>The dot product value.</returns>
		public static float DotProduct(FVector2 u, FVector2 v)
		{
			return (u.X * v.X) + (u.Y * v.Y);
		}
		/// <summary>
		/// Negates a vector.
		/// </summary>
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <returns>A new <see cref="FVector2"/> instance containing the negated values.</returns>
		public static FVector2 Negate(FVector2 v)
		{
			return new FVector2(-v.X, -v.Y);
		}
		/// <summary>
		/// Tests whether two vectors are approximately equal using default tolerance value.
		/// </summary>
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <param name="u">A <see cref="FVector2"/> instance.</param>
		/// <returns>True if the two vectors are approximately equal; otherwise, False.</returns>
		public static bool ApproxEqual(FVector2 v, FVector2 u)
		{
            return ApproxEqual(v, u, CMath.Constants.Epsilon);
		}
		/// <summary>
		/// Tests whether two vectors are approximately equal given a tolerance value.
		/// </summary>
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <param name="u">A <see cref="FVector2"/> instance.</param>
		/// <param name="tolerance">The tolerance value used to test approximate equality.</param>
		/// <returns>True if the two vectors are approximately equal; otherwise, False.</returns>
		public static bool ApproxEqual(FVector2 v, FVector2 u, float tolerance)
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
		/// Initialize FVector2 with X and Y
		/// </summary>
		/// <param name="inX"></param>
		/// <param name="inY"></param>
		public void Set(float inX, float inY)
		{
			mX = inX;
			mY = inY;
		}

		/// <summary>
		/// Scale the vector so that its length is 1.
		/// </summary>
		public void Normalize()
		{
			var length = GetLength();
			if (length == 0)
			{
				throw new DivideByZeroException("Trying to normalize a vector with length of zero.");
			}

			mX /= length;
			mY /= length;

		}
		/// <summary>
		/// Returns the length of the vector.
		/// </summary>
		/// <returns>The length of the vector. (Sqrt(X*X + Y*Y))</returns>
		public float GetLength()
		{
			return (float)System.Math.Sqrt(mX*mX + mY*mY);
		}
		/// <summary>
		/// Returns the squared length of the vector.
		/// </summary>
		/// <returns>The squared length of the vector. (X*X + Y*Y)</returns>
		public float GetLengthSquared()
		{
			return (mX*mX + mY*mY);
		}
		#endregion

		#region Overrides
		/// <summary>
		/// Returns the hashcode for this instance.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			return mX.GetHashCode() ^ mY.GetHashCode();
		}
		/// <summary>
		/// Returns a value indicating whether this instance is equal to
		/// the specified object.
		/// </summary>
		/// <param name="obj">An object to compare to this instance.</param>
		/// <returns>True if <paramref name="obj"/> is a <see cref="FVector2"/> and has the same values as this instance; otherwise, False.</returns>
		public override bool Equals(object obj)
		{
			if (obj is FVector2)
			{
				var v = (FVector2)obj;
				return (mX == v.X) && (mY == v.Y);
			}
			return false;
		}

		/// <summary>
		/// Returns a string representation of this object.
		/// </summary>
		/// <returns>A string representation of this object.</returns>
		public override string ToString()
		{
			return string.Format("({0}, {1})", mX, mY);
		}
		#endregion

		#region Comparison Operators
		/// <summary>
		/// Tests whether two specified vectors are equal.
		/// </summary>
		/// <param name="u">The left-hand vector.</param>
		/// <param name="v">The right-hand vector.</param>
		/// <returns>True if the two vectors are equal; otherwise, False.</returns>
		public static bool operator==(FVector2 u, FVector2 v)
		{
			if (Object.Equals(u, null))
			{
				return Object.Equals(v, null);
			}

			if (Object.Equals(v, null))
			{
				return Object.Equals(u, null);
			}

			return (u.X == v.X) && (u.Y == v.Y);
		}
		/// <summary>
		/// Tests whether two specified vectors are not equal.
		/// </summary>
		/// <param name="u">The left-hand vector.</param>
		/// <param name="v">The right-hand vector.</param>
		/// <returns>True if the two vectors are not equal; otherwise, False.</returns>
		public static bool operator!=(FVector2 u, FVector2 v)
		{
			if (Object.Equals(u, null))
			{
				return !Object.Equals(v, null);
			}

			if (Object.Equals(v, null))
			{
				return !Object.Equals(u, null);
			}

			return !((u.X == v.X) && (u.Y == v.Y));
		}

		/// <summary>
		/// Tests if a vector's components are greater than another vector's components.
		/// </summary>
		/// <param name="u">The left-hand vector.</param>
		/// <param name="v">The right-hand vector.</param>
		/// <returns>True if the left-hand vector's components are greater than the right-hand vector's component; otherwise, False.</returns>
		public static bool operator>(FVector2 u, FVector2 v)
		{
			return (
				(u.mX > v.mX) &&
				(u.mY > v.mY));
		}
		/// <summary>
		/// Tests if a vector's components are smaller than another vector's components.
		/// </summary>
		/// <param name="u">The left-hand vector.</param>
		/// <param name="v">The right-hand vector.</param>
		/// <returns>True if the left-hand vector's components are smaller than the right-hand vector's component; otherwise, False.</returns>
		public static bool operator<(FVector2 u, FVector2 v)
		{
			return (
				(u.mX < v.mX) &&
				(u.mY < v.mY));
		}
		/// <summary>
		/// Tests if a vector's components are greater or equal than another vector's components.
		/// </summary>
		/// <param name="u">The left-hand vector.</param>
		/// <param name="v">The right-hand vector.</param>
		/// <returns>True if the left-hand vector's components are greater or equal than the right-hand vector's component; otherwise, False.</returns>
		public static bool operator>=(FVector2 u, FVector2 v)
		{
			return (
				(u.mX >= v.mX) &&
				(u.mY >= v.mY));
		}
		/// <summary>
		/// Tests if a vector's components are smaller or equal than another vector's components.
		/// </summary>
		/// <param name="u">The left-hand vector.</param>
		/// <param name="v">The right-hand vector.</param>
		/// <returns>True if the left-hand vector's components are smaller or equal than the right-hand vector's component; otherwise, False.</returns>
		public static bool operator<=(FVector2 u, FVector2 v)
		{
			return (
				(u.mX <= v.mX) &&
				(u.mY <= v.mY));
		}
		#endregion

		#region Unary Operators
		/// <summary>
		/// Negates the values of the vector.
		/// </summary>
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <returns>A new <see cref="FVector2"/> instance containing the negated values.</returns>
		public static FVector2 operator-(FVector2 v)
		{
			return FVector2.Negate(v);
		}
		#endregion

		#region Binary Operators
		/// <summary>
		/// Adds two vectors.
		/// </summary>
		/// <param name="u">A <see cref="FVector2"/> instance.</param>
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <returns>A new <see cref="FVector2"/> instance containing the sum.</returns>
		public static FVector2 operator+(FVector2 u, FVector2 v)
		{
			return FVector2.Add(u,v);
		}
		/// <summary>
		/// Adds a vector and a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector2"/> instance containing the sum.</returns>
		public static FVector2 operator+(FVector2 v, float s)
		{
			return FVector2.Add(v,s);
		}
		/// <summary>
		/// Adds a vector and a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector2"/> instance containing the sum.</returns>
		public static FVector2 operator+(float s, FVector2 v)
		{
			return FVector2.Add(v,s);
		}
		/// <summary>
		/// Subtracts a vector from a vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector2"/> instance.</param>
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <returns>A new <see cref="FVector2"/> instance containing the difference.</returns>
		/// <remarks>
		///	result[i] = v[i] - w[i].
		/// </remarks>
		public static FVector2 operator-(FVector2 u, FVector2 v)
		{
			return FVector2.Subtract(u,v);
		}
		/// <summary>
		/// Subtracts a scalar from a vector.
		/// </summary>
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector2"/> instance containing the difference.</returns>
		/// <remarks>
		/// result[i] = v[i] - s
		/// </remarks>
		public static FVector2 operator-(FVector2 v, float s)
		{
			return FVector2.Subtract(v, s);
		}
		/// <summary>
		/// Subtracts a vector from a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector2"/> instance containing the difference.</returns>
		/// <remarks>
		/// result[i] = s - v[i]
		/// </remarks>
		public static FVector2 operator-(float s, FVector2 v)
		{
			return FVector2.Subtract(s, v);
		}
		/// <summary>
		/// Multiplies a vector by a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector2"/> containing the result.</returns>
		public static FVector2 operator *(FVector2 u, FVector2 v)
		{
			return FVector2.Multiply(u, v);
		}
		/// <summary>
		/// Multiplies a vector by a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector2"/> containing the result.</returns>
		public static FVector2 operator*(FVector2 v, float s)
		{
			return FVector2.Multiply(v,s);
		}
		/// <summary>
		/// Multiplies a vector by a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector2"/> containing the result.</returns>
		public static FVector2 operator*(float s, FVector2 v)
		{
			return FVector2.Multiply(v,s);
		}
		/// <summary>
		/// Divides a vector by a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <returns>A new <see cref="FVector2"/> containing the quotient.</returns>
		/// <remarks>
		/// result[i] = v[i] / s;
		/// </remarks>
		public static FVector2 operator/(FVector2 v, float s)
		{
			return FVector2.Divide(v,s);
		}
		/// <summary>
		/// Divides a scalar by a vector.
		/// </summary>
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <returns>A new <see cref="FVector2"/> containing the quotient.</returns>
		/// <remarks>
		/// result[i] = s / v[i]
		/// </remarks>
		public static FVector2 operator/(float s, FVector2 v)
		{
			return FVector2.Divide(s,v);
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
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <returns>An array of single-precision floating point values.</returns>
		public static explicit operator float[](FVector2 v)
		{
			var array = new float[2];
			array[0] = v.X;
			array[1] = v.Y;
			return array;
		}
		/// <summary>
		/// Converts the vector to an array of single-precision floating point values.
		/// </summary>
		/// <param name="v">A <see cref="FVector2"/> instance.</param>
		/// <returns>An array of single-precision floating point values.</returns>
        public static explicit operator List<float>(FVector2 v)
		{
            var array = new List<float>(2);
			array[0] = v.X;
			array[1] = v.Y;
			return array;
		}
		#endregion
	}

}
