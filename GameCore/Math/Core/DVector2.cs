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
	/// Represents 2-Dimentional vector of double-precision floating point numbers.
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	[StructLayout(LayoutKind.Sequential)]
	public struct DVector2 : ICloneable
	{
		#region Private fields
		internal double mX;
		internal double mY;
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="DVector2"/> class with the specified coordinates.
		/// </summary>
		/// <param name="x">The vector's X coordinate.</param>
		/// <param name="y">The vector's Y coordinate.</param>
		public DVector2(double x, double y)
		{
			mX = x;
			mY = y;
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="DVector2"/> class with the specified coordinates.
		/// </summary>
		/// <param name="coordinates">An array containing the coordinate parameters.</param>
		public DVector2(double[] coordinates)
		{
			Debug.Assert(coordinates != null);
			Debug.Assert(coordinates.Length >= 2);

			mX = coordinates[0];
			mY = coordinates[1];
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="DVector2"/> class with the specified coordinates.
		/// </summary>
		/// <param name="coordinates">An array containing the coordinate parameters.</param>
        public DVector2(List<double> coordinates)
		{
			Debug.Assert(coordinates != null);
			Debug.Assert(coordinates.Count >= 2);

			mX = coordinates[0];
			mY = coordinates[1];
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="DVector2"/> class using coordinates from a given <see cref="DVector2"/> instance.
		/// </summary>
		/// <param name="vector">A <see cref="DVector2"/> to get the coordinates from.</param>
		public DVector2(DVector2 vector)
		{
			mX = vector.X;
			mY = vector.Y;
		}
		#endregion

		#region Constants
		/// <summary>
		/// 2-Dimentional double-precision floating point zero vector.
		/// </summary>
		public static readonly DVector2 sZero	= new DVector2(0.0f, 0.0f);
		/// <summary>
		/// 2-Dimentional double-precision floating point X-Axis vector.
		/// </summary>
		public static readonly DVector2 sAxisX	= new DVector2(1.0f, 0.0f);
		/// <summary>
		/// 2-Dimentional double-precision floating point Y-Axis vector.
		/// </summary>
		public static readonly DVector2 sAxisY	= new DVector2(0.0f, 1.0f);
		#endregion

		#region Public properties
		/// <summery>
		/// Gets or sets the x-coordinate of this vector.
		/// </summery>
		/// <value>The x-coordinate of this vector.</value>
		public double X
		{
			get { return mX; }
			set { mX = value;}
		}
		/// <summery>
		/// Gets or sets the y-coordinate of this vector.
		/// </summery>
		/// <value>The y-coordinate of this vector.</value>
		public double Y
		{
			get { return mY; }
			set { mY = value;}
		}
		#endregion

		#region ICloneable Members
		/// <summary>
		/// Creates an exact copy of this <see cref="DVector2"/> object.
		/// </summary>
		/// <returns>The <see cref="DVector2"/> object this method creates, cast as an object.</returns>
		object ICloneable.Clone()
		{
			return new DVector2(this);
		}
		/// <summary>
		/// Creates an exact copy of this <see cref="DVector2"/> object.
		/// </summary>
		/// <returns>The <see cref="DVector2"/> object this method creates.</returns>
		public DVector2 Clone()
		{
			return new DVector2(this);
		}
		#endregion

		#region Public Static Parse Methods
		/// <summary>
		/// Converts the specified string to its <see cref="DVector2"/> equivalent.
		/// </summary>
		/// <param name="s">A string representation of a <see cref="DVector2"/></param>
		/// <returns>A <see cref="DVector2"/> that represents the vector specified by the <paramref name="s"/> parameter.</returns>
		public static DVector2 Parse(string s)
		{
			var r = new Regex(@"\((?<x>.*),(?<y>.*)\)", RegexOptions.None);
			var m = r.Match(s);
			if (m.Success)
			{
				return new DVector2(
					double.Parse(m.Result("${x}")),
					double.Parse(m.Result("${y}"))
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
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <param name="w">A <see cref="DVector2"/> instance.</param>
		/// <returns>A new <see cref="DVector2"/> instance containing the sum.</returns>
		public static DVector2 Add(DVector2 v, DVector2 w)
		{
			return new DVector2(v.X + w.X, v.Y + w.Y);
		}
		/// <summary>
		/// Adds a vector and a scalar.
		/// </summary>
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="DVector2"/> instance containing the sum.</returns>
		public static DVector2 Add(DVector2 v, double s)
		{
			return new DVector2(v.X + s, v.Y + s);
		}
		/// <summary>
		/// Adds two vectors and put the result in the third vector.
		/// </summary>
		/// <param name="u">A <see cref="DVector2"/> instance.</param>
		/// <param name="v">A <see cref="DVector2"/> instance</param>
		/// <param name="w">A <see cref="DVector2"/> instance to hold the result.</param>
		public static void Add(DVector2 u, DVector2 v, DVector2 w)
		{
			w.X = u.X + v.X;
			w.Y = u.Y + v.Y;
		}
		/// <summary>
		/// Adds a vector and a scalar and put the result into another vector.
		/// </summary>
		/// <param name="u">A <see cref="DVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <param name="v">A <see cref="DVector2"/> instance to hold the result.</param>
		public static void Add(DVector2 u, double s, DVector2 v)
		{
			v.X = u.X + s;
			v.Y = u.Y + s;
		}
		/// <summary>
		/// Subtracts a vector from a vector.
		/// </summary>
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <param name="w">A <see cref="DVector2"/> instance.</param>
		/// <returns>A new <see cref="DVector2"/> instance containing the difference.</returns>
		/// <remarks>
		///	result[i] = v[i] - w[i].
		/// </remarks>
		public static DVector2 Subtract(DVector2 v, DVector2 w)
		{
			return new DVector2(v.X - w.X, v.Y - w.Y);
		}
		/// <summary>
		/// Subtracts a scalar from a vector.
		/// </summary>
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="DVector2"/> instance containing the difference.</returns>
		/// <remarks>
		/// result[i] = v[i] - s
		/// </remarks>
		public static DVector2 Subtract(DVector2 v, double s)
		{
			return new DVector2(v.X - s, v.Y - s);
		}
		/// <summary>
		/// Subtracts a vector from a scalar.
		/// </summary>
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="DVector2"/> instance containing the difference.</returns>
		/// <remarks>
		/// result[i] = s - v[i]
		/// </remarks>
		public static DVector2 Subtract(double s, DVector2 v)
		{
			return new DVector2(s - v.X, s - v.Y);
		}
		/// <summary>
		/// Subtracts a vector from a second vector and puts the result into a third vector.
		/// </summary>
		/// <param name="u">A <see cref="DVector2"/> instance.</param>
		/// <param name="v">A <see cref="DVector2"/> instance</param>
		/// <param name="w">A <see cref="DVector2"/> instance to hold the result.</param>
		/// <remarks>
		///	w[i] = v[i] - w[i].
		/// </remarks>
		public static void Subtract(DVector2 u, DVector2 v, DVector2 w)
		{
			w.X = u.X - v.X;
			w.Y = u.Y - v.Y;
		}
		/// <summary>
		/// Subtracts a vector from a scalar and put the result into another vector.
		/// </summary>
		/// <param name="u">A <see cref="DVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <param name="v">A <see cref="DVector2"/> instance to hold the result.</param>
		/// <remarks>
		/// v[i] = u[i] - s
		/// </remarks>
		public static void Subtract(DVector2 u, double s, DVector2 v)
		{
			v.X = u.X - s;
			v.Y = u.Y - s;
		}
		/// <summary>
		/// Subtracts a scalar from a vector and put the result into another vector.
		/// </summary>
		/// <param name="u">A <see cref="DVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <param name="v">A <see cref="DVector2"/> instance to hold the result.</param>
		/// <remarks>
		/// v[i] = s - u[i]
		/// </remarks>
		public static void Subtract(double s, DVector2 u, DVector2 v)
		{
			v.X = s - u.X;
			v.Y = s - u.Y;
		}
		/// <summary>
		/// Divides a vector by another vector.
		/// </summary>
		/// <param name="u">A <see cref="DVector2"/> instance.</param>
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <returns>A new <see cref="DVector2"/> containing the quotient.</returns>
		/// <remarks>
		///	result[i] = u[i] / v[i].
		/// </remarks>
		public static DVector2 Divide(DVector2 u, DVector2 v)
		{
			return new DVector2(u.X / v.X, u.Y / v.Y);
		}
		/// <summary>
		/// Divides a vector by a scalar.
		/// </summary>
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <returns>A new <see cref="DVector2"/> containing the quotient.</returns>
		/// <remarks>
		/// result[i] = v[i] / s;
		/// </remarks>
		public static DVector2 Divide(DVector2 v, double s)
		{
			return new DVector2(v.X / s, v.Y / s);
		}
		/// <summary>
		/// Divides a scalar by a vector.
		/// </summary>
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <returns>A new <see cref="DVector2"/> containing the quotient.</returns>
		/// <remarks>
		/// result[i] = s / v[i]
		/// </remarks>
		public static DVector2 Divide(double s, DVector2 v)
		{
			return new DVector2(s / v.X, s/ v.Y);
		}
		/// <summary>
		/// Divides a vector by another vector.
		/// </summary>
		/// <param name="u">A <see cref="DVector2"/> instance.</param>
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <param name="w">A <see cref="DVector2"/> instance to hold the result.</param>
		/// <remarks>
		/// w[i] = u[i] / v[i]
		/// </remarks>
		public static void Divide(DVector2 u, DVector2 v, DVector2 w)
		{
			w.X = u.X / v.X;
			w.Y = u.Y / v.Y;
		}
		/// <summary>
		/// Divides a vector by a scalar.
		/// </summary>
		/// <param name="u">A <see cref="DVector2"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <param name="v">A <see cref="DVector2"/> instance to hold the result.</param>
		/// <remarks>
		/// v[i] = u[i] / s
		/// </remarks>
		public static void Divide(DVector2 u, double s, DVector2 v)
		{
			v.X = u.X / s;
			v.Y = u.Y / s;
		}
		/// <summary>
		/// Divides a scalar by a vector.
		/// </summary>
		/// <param name="u">A <see cref="DVector2"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <param name="v">A <see cref="DVector2"/> instance to hold the result.</param>
		/// <remarks>
		/// v[i] = s / u[i]
		/// </remarks>
		public static void Divide(double s, DVector2 u, DVector2 v)
		{
			v.X = s / u.X;
			v.Y = s / u.Y;
		}
		/// <summary>
		/// Multiplies a vector by a scalar.
		/// </summary>
		/// <param name="u">A <see cref="DVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="DVector2"/> containing the result.</returns>
		public static DVector2 Multiply(DVector2 u, double s)
		{
			return new DVector2(u.X * s, u.Y * s);
		}
		/// <summary>
		/// Multiplies a vector by a scalar and put the result in another vector.
		/// </summary>
		/// <param name="u">A <see cref="DVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <param name="v">A <see cref="DVector2"/> instance to hold the result.</param>
		public static void Multiply(DVector2 u, double s, DVector2 v)
		{
			v.X = u.X * s;
			v.Y = u.Y * s;
		}
		/// <summary>
		/// Calculates the dot product of two vectors.
		/// </summary>
		/// <param name="u">A <see cref="DVector2"/> instance.</param>
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <returns>The dot product value.</returns>
		public static double DotProduct(DVector2 u, DVector2 v)
		{
			return (u.X * v.X) + (u.Y * v.Y);
		}
		/// <summary>
		/// Negates a vector.
		/// </summary>
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <returns>A new <see cref="DVector2"/> instance containing the negated values.</returns>
		public static DVector2 Negate(DVector2 v)
		{
			return new DVector2(-v.X, -v.Y);
		}
		/// <summary>
		/// Tests whether two vectors are approximately equal using default tolerance value.
		/// </summary>
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <param name="u">A <see cref="DVector2"/> instance.</param>
		/// <returns>True if the two vectors are approximately equal; otherwise, False.</returns>
		public static bool ApproxEqual(DVector2 v, DVector2 u)
		{
            return ApproxEqual(v, u, CMath.Constants.EpsilonD);
		}
		/// <summary>
		/// Tests whether two vectors are approximately equal given a tolerance value.
		/// </summary>
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <param name="u">A <see cref="DVector2"/> instance.</param>
		/// <param name="tolerance">The tolerance value used to test approximate equality.</param>
		/// <returns>True if the two vectors are approximately equal; otherwise, False.</returns>
		public static bool ApproxEqual(DVector2 v, DVector2 u, double tolerance)
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
		public double GetLength()
		{
			return System.Math.Sqrt(mX*mX + mY*mY);
		}
		/// <summary>
		/// Returns the squared length of the vector.
		/// </summary>
		/// <returns>The squared length of the vector. (X*X + Y*Y)</returns>
		public double GetLengthSquared()
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
		/// <returns>True if <paramref name="obj"/> is a <see cref="DVector2"/> and has the same values as this instance; otherwise, False.</returns>
		public override bool Equals(object obj)
		{
			if (obj is DVector2)
			{
				var v = (DVector2)obj;
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
		public static bool operator==(DVector2 u, DVector2 v)
		{
			if (object.Equals(u, null))
			{
				return object.Equals(v, null);
			}

			if (object.Equals(v, null))
			{
				return object.Equals(u, null);
			}

			return (u.X == v.X) && (u.Y == v.Y);
		}
		/// <summary>
		/// Tests whether two specified vectors are not equal.
		/// </summary>
		/// <param name="u">The left-hand vector.</param>
		/// <param name="v">The right-hand vector.</param>
		/// <returns>True if the two vectors are not equal; otherwise, False.</returns>
		public static bool operator!=(DVector2 u, DVector2 v)
		{
			if (object.Equals(u, null))
			{
				return !object.Equals(v, null);
			}

			if (object.Equals(v, null))
			{
				return !object.Equals(u, null);
			}

			return !((u.X == v.X) && (u.Y == v.Y));
		}

		/// <summary>
		/// Tests if a vector's components are greater than another vector's components.
		/// </summary>
		/// <param name="u">The left-hand vector.</param>
		/// <param name="v">The right-hand vector.</param>
		/// <returns>True if the left-hand vector's components are greater than the right-hand vector's component; otherwise, False.</returns>
		public static bool operator>(DVector2 u, DVector2 v)
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
		public static bool operator<(DVector2 u, DVector2 v)
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
		public static bool operator>=(DVector2 u, DVector2 v)
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
		public static bool operator<=(DVector2 u, DVector2 v)
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
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <returns>A new <see cref="DVector2"/> instance containing the negated values.</returns>
		public static DVector2 operator-(DVector2 v)
		{
			return DVector2.Negate(v);
		}
		#endregion

		#region Binary Operators
		/// <summary>
		/// Adds two vectors.
		/// </summary>
		/// <param name="u">A <see cref="DVector2"/> instance.</param>
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <returns>A new <see cref="DVector2"/> instance containing the sum.</returns>
		public static DVector2 operator+(DVector2 u, DVector2 v)
		{
			return DVector2.Add(u,v);
		}
		/// <summary>
		/// Adds a vector and a scalar.
		/// </summary>
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="DVector2"/> instance containing the sum.</returns>
		public static DVector2 operator+(DVector2 v, double s)
		{
			return DVector2.Add(v,s);
		}
		/// <summary>
		/// Adds a vector and a scalar.
		/// </summary>
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="DVector2"/> instance containing the sum.</returns>
		public static DVector2 operator+(double s, DVector2 v)
		{
			return DVector2.Add(v,s);
		}
		/// <summary>
		/// Subtracts a vector from a vector.
		/// </summary>
		/// <param name="u">A <see cref="DVector2"/> instance.</param>
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <returns>A new <see cref="DVector2"/> instance containing the difference.</returns>
		/// <remarks>
		///	result[i] = v[i] - w[i].
		/// </remarks>
		public static DVector2 operator-(DVector2 u, DVector2 v)
		{
			return DVector2.Subtract(u,v);
		}
		/// <summary>
		/// Subtracts a scalar from a vector.
		/// </summary>
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="DVector2"/> instance containing the difference.</returns>
		/// <remarks>
		/// result[i] = v[i] - s
		/// </remarks>
		public static DVector2 operator-(DVector2 v, double s)
		{
			return DVector2.Subtract(v, s);
		}
		/// <summary>
		/// Subtracts a vector from a scalar.
		/// </summary>
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="DVector2"/> instance containing the difference.</returns>
		/// <remarks>
		/// result[i] = s - v[i]
		/// </remarks>
		public static DVector2 operator-(double s, DVector2 v)
		{
			return DVector2.Subtract(s, v);
		}

		/// <summary>
		/// Multiplies a vector by a scalar.
		/// </summary>
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="DVector2"/> containing the result.</returns>
		public static DVector2 operator*(DVector2 v, double s)
		{
			return DVector2.Multiply(v,s);
		}
		/// <summary>
		/// Multiplies a vector by a scalar.
		/// </summary>
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="DVector2"/> containing the result.</returns>
		public static DVector2 operator*(double s, DVector2 v)
		{
			return DVector2.Multiply(v,s);
		}
		/// <summary>
		/// Divides a vector by a scalar.
		/// </summary>
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <returns>A new <see cref="DVector2"/> containing the quotient.</returns>
		/// <remarks>
		/// result[i] = v[i] / s;
		/// </remarks>
		public static DVector2 operator/(DVector2 v, double s)
		{
			return DVector2.Divide(v,s);
		}
		/// <summary>
		/// Divides a scalar by a vector.
		/// </summary>
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <returns>A new <see cref="DVector2"/> containing the quotient.</returns>
		/// <remarks>
		/// result[i] = s / v[i]
		/// </remarks>
		public static DVector2 operator/(double s, DVector2 v)
		{
			return DVector2.Divide(s,v);
		}
		#endregion

		#region Array Indexing Operator
		/// <summary>
		/// Indexer ( [x, y] ).
		/// </summary>
		public double this[int index]
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
		/// Converts the vector to an array of double-precision floating point values.
		/// </summary>
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <returns>An array of double-precision floating point values.</returns>
		public static explicit operator double[](DVector2 v)
		{
			var array = new double[2];
			array[0] = v.X;
			array[1] = v.Y;
			return array;
		}
		/// <summary>
		/// Converts the vector to an array of double-precision floating point values.
		/// </summary>
		/// <param name="v">A <see cref="DVector2"/> instance.</param>
		/// <returns>An array of double-precision floating point values.</returns>
        public static explicit operator List<double>(DVector2 v)
		{
            var array = new List<double>(2);
			array[0] = v.X;
			array[1] = v.Y;
			return array;
		}
		#endregion

	}

}
