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

namespace Core
{
	/// <summary>
	/// Represents 3-Dimentional vector of double-precision floating point numbers.
	/// </summary>
	[Serializable]
	[TypeConverter(typeof(ExpandableObjectConverter))]
	[StructLayout(LayoutKind.Sequential)]
	public struct DVector3 : ISerializable, ICloneable
	{
		#region Private fields
		internal double mX;
		internal double mY;
		internal double mZ;
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="DVector3"/> class with the specified coordinates.
		/// </summary>
		/// <param name="x">The vector's X coordinate.</param>
		/// <param name="y">The vector's Y coordinate.</param>
		/// <param name="z">The vector's Z coordinate.</param>
		public DVector3(double x, double y, double z)
		{
			mX = x;
			mY = y;
			mZ = z;
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="DVector3"/> class with the specified coordinates.
		/// </summary>
		/// <param name="coordinates">An array containing the coordinate parameters.</param>
		public DVector3(double[] coordinates)
		{
			Debug.Assert(coordinates != null);
			Debug.Assert(coordinates.Length >= 3);

			mX = coordinates[0];
			mY = coordinates[1];
			mZ = coordinates[2];
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="DVector3"/> class with the specified coordinates.
		/// </summary>
		/// <param name="coordinates">An array containing the coordinate parameters.</param>
        public DVector3(List<double> coordinates)
		{
			Debug.Assert(coordinates != null);
			Debug.Assert(coordinates.Count >= 3);

			mX = coordinates[0];
			mY = coordinates[1];
			mZ = coordinates[2];
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="DVector3"/> class using coordinates from a given <see cref="DVector3"/> instance.
		/// </summary>
		/// <param name="vector">A <see cref="DVector3"/> to get the coordinates from.</param>
		public DVector3(DVector3 vector)
		{
			mX = vector.X;
			mY = vector.Y;
			mZ = vector.Z;
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="DVector3"/> class with serialized data.
		/// </summary>
		/// <param name="info">The object that holds the serialized object data.</param>
		/// <param name="context">The contextual information about the source or destination.</param>
		private DVector3(SerializationInfo info, StreamingContext context)
		{
			mX = info.GetSingle("X");
			mY = info.GetSingle("Y");
			mZ = info.GetSingle("Z");
		}
		#endregion

		#region Constants
		/// <summary>
		/// 3-Dimentional double-precision floating point zero vector.
		/// </summary>
		public static readonly DVector3 sZero	= new DVector3(0.0f, 0.0f, 0.0f);
		/// <summary>
		/// 3-Dimentional double-precision floating point X-Axis vector.
		/// </summary>
		public static readonly DVector3 sAxisX	= new DVector3(1.0f, 0.0f, 0.0f);
		/// <summary>
		/// 3-Dimentional double-precision floating point Y-Axis vector.
		/// </summary>
		public static readonly DVector3 sAxisY	= new DVector3(0.0f, 1.0f, 0.0f);
		/// <summary>
		/// 3-Dimentional double-precision floating point Y-Axis vector.
		/// </summary>
		public static readonly DVector3 sAxisZ	= new DVector3(0.0f, 0.0f, 1.0f);
		#endregion

		#region Public properties
		/// <summary>
		/// Gets or sets the x-coordinate of this vector.
		/// </summary>
		public double X
		{
			get { return mX; }
			set { mX = value;}
		}
		/// <summary>
		/// Gets or sets the y-coordinate of this vector.
		/// </summary>
		public double Y
		{
			get { return mY; }
			set { mY = value;}
		}
		/// <summary>
		/// Gets or sets the z-coordinate of this vector.
		/// </summary>
		public double Z
		{
			get { return mZ; }
			set { mZ = value;}
		}
		#endregion

		#region ICloneable Members
		/// <summary>
		/// Creates an exact copy of this <see cref="DVector3"/> object.
		/// </summary>
		/// <returns>The <see cref="DVector3"/> object this method creates, cast as an object.</returns>
		object ICloneable.Clone()
		{
			return new DVector3(this);
		}
		/// <summary>
		/// Creates an exact copy of this <see cref="DVector3"/> object.
		/// </summary>
		/// <returns>The <see cref="DVector3"/> object this method creates.</returns>
		public DVector3 Clone()
		{
			return new DVector3(this);
		}
		#endregion

		#region ISerializable Members
		/// <summary>
		/// Populates a <see cref="SerializationInfo"/> with the data needed to serialize this object.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo"/> to populate with data. </param>
		/// <param name="context">The destination (see <see cref="StreamingContext"/>) for this serialization.</param>
		[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter=true)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("X", mX);
			info.AddValue("Y", mY);
			info.AddValue("Z", mZ);
		}
		#endregion

		#region Public Static Parse Methods
		/// <summary>
		/// Converts the specified string to its <see cref="DVector3"/> equivalent.
		/// </summary>
		/// <param name="s">A string representation of a <see cref="DVector3"/></param>
		/// <returns>A <see cref="DVector3"/> that represents the vector specified by the <paramref name="s"/> parameter.</returns>
		public static DVector3 Parse(string s)
		{
			Regex r = new Regex(@"\((?<x>.*),(?<y>.*),(?<z>.*)\)", RegexOptions.None);
			Match m = r.Match(s);
			if (m.Success)
			{
				return new DVector3(
					double.Parse(m.Result("${x}")),
					double.Parse(m.Result("${y}")),
					double.Parse(m.Result("${z}"))
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
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <param name="w">A <see cref="DVector3"/> instance.</param>
		/// <returns>A new <see cref="DVector3"/> instance containing the sum.</returns>
		public static DVector3 Add(DVector3 v, DVector3 w)
		{
			return new DVector3(v.X + w.X, v.Y + w.Y, v.Z + w.Z);
		}
		/// <summary>
		/// Adds a vector and a scalar.
		/// </summary>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="DVector3"/> instance containing the sum.</returns>
		public static DVector3 Add(DVector3 v, double s)
		{
			return new DVector3(v.X + s, v.Y + s, v.Z + s);
		}
		/// <summary>
		/// Adds two vectors and put the result in the third vector.
		/// </summary>
		/// <param name="u">A <see cref="DVector3"/> instance.</param>
		/// <param name="v">A <see cref="DVector3"/> instance</param>
		/// <param name="w">A <see cref="DVector3"/> instance to hold the result.</param>
		public static void Add(DVector3 u, DVector3 v, DVector3 w)
		{
			w.X = u.X + v.X;
			w.Y = u.Y + v.Y;
			w.Z = u.Z + v.Z;
		}
		/// <summary>
		/// Adds a vector and a scalar and put the result into another vector.
		/// </summary>
		/// <param name="u">A <see cref="DVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <param name="v">A <see cref="DVector3"/> instance to hold the result.</param>
		public static void Add(DVector3 u, double s, DVector3 v)
		{
			v.X = u.X + s;
			v.Y = u.Y + s;
			v.Z = u.Z + s;
		}
		/// <summary>
		/// Subtracts a vector from a vector.
		/// </summary>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <param name="w">A <see cref="DVector3"/> instance.</param>
		/// <returns>A new <see cref="DVector3"/> instance containing the difference.</returns>
		/// <remarks>
		///	result[i] = v[i] - w[i].
		/// </remarks>
		public static DVector3 Subtract(DVector3 v, DVector3 w)
		{
			return new DVector3(v.X - w.X, v.Y - w.Y, v.Z - w.Z);
		}
		/// <summary>
		/// Subtracts a scalar from a vector.
		/// </summary>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="DVector3"/> instance containing the difference.</returns>
		/// <remarks>
		/// result[i] = v[i] - s
		/// </remarks>
		public static DVector3 Subtract(DVector3 v, double s)
		{
			return new DVector3(v.X - s, v.Y - s, v.Z - s);
		}
		/// <summary>
		/// Subtracts a vector from a scalar.
		/// </summary>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="DVector3"/> instance containing the difference.</returns>
		/// <remarks>
		/// result[i] = s - v[i]
		/// </remarks>
		public static DVector3 Subtract(double s, DVector3 v)
		{
			return new DVector3(s - v.X, s - v.Y, s - v.Z);
		}
		/// <summary>
		/// Subtracts a vector from a second vector and puts the result into a third vector.
		/// </summary>
		/// <param name="u">A <see cref="DVector3"/> instance.</param>
		/// <param name="v">A <see cref="DVector3"/> instance</param>
		/// <param name="w">A <see cref="DVector3"/> instance to hold the result.</param>
		/// <remarks>
		///	w[i] = v[i] - w[i].
		/// </remarks>
		public static void Subtract(DVector3 u, DVector3 v, DVector3 w)
		{
			w.X = u.X - v.X;
			w.Y = u.Y - v.Y;
			w.Z = u.Z - v.Z;
		}
		/// <summary>
		/// Subtracts a vector from a scalar and put the result into another vector.
		/// </summary>
		/// <param name="u">A <see cref="DVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <param name="v">A <see cref="DVector3"/> instance to hold the result.</param>
		/// <remarks>
		/// v[i] = u[i] - s
		/// </remarks>
		public static void Subtract(DVector3 u, double s, DVector3 v)
		{
			v.X = u.X - s;
			v.Y = u.Y - s;
			v.Z = u.Z - s;
		}
		/// <summary>
		/// Subtracts a scalar from a vector and put the result into another vector.
		/// </summary>
		/// <param name="u">A <see cref="DVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <param name="v">A <see cref="DVector3"/> instance to hold the result.</param>
		/// <remarks>
		/// v[i] = s - u[i]
		/// </remarks>
		public static void Subtract(double s, DVector3 u, DVector3 v)
		{
			v.X = s - u.X;
			v.Y = s - u.Y;
			v.Z = s - u.Z;
		}
		/// <summary>
		/// Divides a vector by another vector.
		/// </summary>
		/// <param name="u">A <see cref="DVector3"/> instance.</param>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <returns>A new <see cref="DVector3"/> containing the quotient.</returns>
		/// <remarks>
		///	result[i] = u[i] / v[i].
		/// </remarks>
		public static DVector3 Divide(DVector3 u, DVector3 v)
		{
			return new DVector3(u.X / v.X, u.Y / v.Y, u.Z / v.Z);
		}
		/// <summary>
		/// Divides a vector by a scalar.
		/// </summary>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <returns>A new <see cref="DVector3"/> containing the quotient.</returns>
		/// <remarks>
		/// result[i] = v[i] / s;
		/// </remarks>
		public static DVector3 Divide(DVector3 v, double s)
		{
			return new DVector3(v.X / s, v.Y / s, v.Z / s);
		}
		/// <summary>
		/// Divides a scalar by a vector.
		/// </summary>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <returns>A new <see cref="DVector3"/> containing the quotient.</returns>
		/// <remarks>
		/// result[i] = s / v[i]
		/// </remarks>
		public static DVector3 Divide(double s, DVector3 v)
		{
			return new DVector3(s / v.X, s/ v.Y, s / v.Z);
		}
		/// <summary>
		/// Divides a vector by another vector.
		/// </summary>
		/// <param name="u">A <see cref="DVector3"/> instance.</param>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <param name="w">A <see cref="DVector3"/> instance to hold the result.</param>
		/// <remarks>
		/// w[i] = u[i] / v[i]
		/// </remarks>
		public static void Divide(DVector3 u, DVector3 v, DVector3 w)
		{
			w.X = u.X / v.X;
			w.Y = u.Y / v.Y;
			w.Z = u.Z / v.Z;
		}
		/// <summary>
		/// Divides a vector by a scalar.
		/// </summary>
		/// <param name="u">A <see cref="DVector3"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <param name="v">A <see cref="DVector3"/> instance to hold the result.</param>
		/// <remarks>
		/// v[i] = u[i] / s
		/// </remarks>
		public static void Divide(DVector3 u, double s, DVector3 v)
		{
			v.X = u.X / s;
			v.Y = u.Y / s;
			v.Z = u.Z / s;
		}
		/// <summary>
		/// Divides a scalar by a vector.
		/// </summary>
		/// <param name="u">A <see cref="DVector3"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <param name="v">A <see cref="DVector3"/> instance to hold the result.</param>
		/// <remarks>
		/// v[i] = s / u[i]
		/// </remarks>
		public static void Divide(double s, DVector3 u, DVector3 v)
		{
			v.X = s / u.X;
			v.Y = s / u.Y;
			v.Z = s / u.Z;
		}
		/// <summary>
		/// Multiplies a vector by a scalar.
		/// </summary>
		/// <param name="u">A <see cref="DVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="DVector3"/> containing the result.</returns>
		public static DVector3 Multiply(DVector3 u, double s)
		{
			return new DVector3(u.X * s, u.Y * s, u.Z * s);
		}
		/// <summary>
		/// Multiplies a vector by a scalar and put the result in another vector.
		/// </summary>
		/// <param name="u">A <see cref="DVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <param name="v">A <see cref="DVector3"/> instance to hold the result.</param>
		public static void Multiply(DVector3 u, double s, DVector3 v)
		{
			v.X = u.X * s;
			v.Y = u.Y * s;
			v.Z = u.Z * s;
		}
		/// <summary>
		/// Calculates the dot product of two vectors.
		/// </summary>
		/// <param name="u">A <see cref="DVector3"/> instance.</param>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <returns>The dot product value.</returns>
		public static double DotProduct(DVector3 u, DVector3 v)
		{
			return (u.X * v.X) + (u.Y * v.Y) + (u.Z * v.Z);
		}
		/// <summary>
		/// Calculates the cross product of two vectors.
		/// </summary>
		/// <param name="u">A <see cref="DVector3"/> instance.</param>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <returns>A new <see cref="DVector3"/> containing the cross product result.</returns>
		public static DVector3 CrossProduct(DVector3 u, DVector3 v)
		{
			return new DVector3( 
				u.Y*v.Z - u.Z*v.Y, 
				u.Z*v.X - u.X*v.Z, 
				u.X*v.Y - u.Y*v.X );
		}
		/// <summary>
		/// Calculates the cross product of two vectors.
		/// </summary>
		/// <param name="u">A <see cref="DVector3"/> instance.</param>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <param name="w">A <see cref="DVector3"/> instance to hold the cross product result.</param>
		public static void CrossProduct(DVector3 u, DVector3 v, DVector3 w)
		{
			w.X = u.Y*v.Z - u.Z*v.Y;
			w.Y = u.Z*v.X - u.X*v.Z;
			w.Z = u.X*v.Y - u.Y*v.X;
		}
		/// <summary>
		/// Negates a vector.
		/// </summary>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <returns>A new <see cref="DVector3"/> instance containing the negated values.</returns>
		public static DVector3 Negate(DVector3 v)
		{
			return new DVector3(-v.X, -v.Y, -v.Z);
		}
		/// <summary>
		/// Tests whether two vectors are approximately equal using default tolerance value.
		/// </summary>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <param name="u">A <see cref="DVector3"/> instance.</param>
		/// <returns>True if the two vectors are approximately equal; otherwise, False.</returns>
		public static bool ApproxEqual(DVector3 v, DVector3 u)
		{
            return ApproxEqual(v, u, Math.EpsilonD);
		}
		/// <summary>
		/// Tests whether two vectors are approximately equal given a tolerance value.
		/// </summary>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <param name="u">A <see cref="DVector3"/> instance.</param>
		/// <param name="tolerance">The tolerance value used to test approximate equality.</param>
		/// <returns>True if the two vectors are approximately equal; otherwise, False.</returns>
		public static bool ApproxEqual(DVector3 v, DVector3 u, double tolerance)
		{
			return
				(
				(System.Math.Abs(v.X - u.X) <= tolerance) &&
				(System.Math.Abs(v.Y - u.Y) <= tolerance) &&
				(System.Math.Abs(v.Z - u.Z) <= tolerance)
				);
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Scale the vector so that its length is 1.
		/// </summary>
		public void Normalize()
		{
			double length = GetLength();
			if (length == 0)
			{
				throw new DivideByZeroException("Trying to normalize a vector with length of zero.");
			}

			mX /= length;
			mY /= length;
			mZ /= length;
		}
		/// <summary>
		/// Returns the length of the vector.
		/// </summary>
		/// <returns>The length of the vector. (Sqrt(X*X + Y*Y + Z*Z))</returns>
		public double GetLength()
		{
			return System.Math.Sqrt(mX*mX + mY*mY + mZ*mZ);
		}
		/// <summary>
		/// Returns the squared length of the vector.
		/// </summary>
		/// <returns>The squared length of the vector. (X*X + Y*Y + Z*Z)</returns>
		public double GetLengthSquared()
		{
			return (mX*mX + mY*mY + mZ*mZ);
		}
		#endregion

		#region Overrides
		/// <summary>
		/// Returns the hashcode for this instance.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			return mX.GetHashCode() ^ mY.GetHashCode() ^ mZ.GetHashCode();
		}
		/// <summary>
		/// Returns a value indicating whether this instance is equal to
		/// the specified object.
		/// </summary>
		/// <param name="obj">An object to compare to this instance.</param>
		/// <returns>True if <paramref name="obj"/> is a <see cref="DVector3"/> and has the same values as this instance; otherwise, False.</returns>
		public override bool Equals(object obj)
		{
			if (obj is DVector3)
			{
				DVector3 v = (DVector3)obj;
				return (mX == v.X) && (mY == v.Y) && (mZ == v.Z);
			}
			return false;
		}

		/// <summary>
		/// Returns a string representation of this object.
		/// </summary>
		/// <returns>A string representation of this object.</returns>
		public override string ToString()
		{
			return string.Format("({0}, {1}, {2})", mX, mY, mZ);
		}
		#endregion
		
		#region Comparison Operators
		/// <summary>
		/// Tests whether two specified vectors are equal.
		/// </summary>
		/// <param name="u">The left-hand vector.</param>
		/// <param name="v">The right-hand vector.</param>
		/// <returns>True if the two vectors are equal; otherwise, False.</returns>
		public static bool operator==(DVector3 u, DVector3 v)
		{
			if (Object.Equals(u, null))
			{
				return Object.Equals(v, null);
			}

			if (Object.Equals(v, null))
			{
				return Object.Equals(u, null);
			}

			return (u.X == v.X) && (u.Y == v.Y) && (u.Z == v.Z);
		}
		/// <summary>
		/// Tests whether two specified vectors are not equal.
		/// </summary>
		/// <param name="u">The left-hand vector.</param>
		/// <param name="v">The right-hand vector.</param>
		/// <returns>True if the two vectors are not equal; otherwise, False.</returns>
		public static bool operator!=(DVector3 u, DVector3 v)
		{
			if (Object.Equals(u, null))
			{
				return !Object.Equals(v, null);
			}

			if (Object.Equals(v, null))
			{
				return !Object.Equals(u, null);
			}

			return !((u.X == v.X) && (u.Y == v.Y) && (u.Z == v.Z));
		}
		/// <summary>
		/// Tests if a vector's components are greater than another vector's components.
		/// </summary>
		/// <param name="u">The left-hand vector.</param>
		/// <param name="v">The right-hand vector.</param>
		/// <returns>True if the left-hand vector's components are greater than the right-hand vector's component; otherwise, False.</returns>
		public static bool operator>(DVector3 u, DVector3 v)
		{
			return (
				(u.mX > v.mX) && 
				(u.mY > v.mY) && 
				(u.mZ > v.mZ));
		}
		/// <summary>
		/// Tests if a vector's components are smaller than another vector's components.
		/// </summary>
		/// <param name="u">The left-hand vector.</param>
		/// <param name="v">The right-hand vector.</param>
		/// <returns>True if the left-hand vector's components are smaller than the right-hand vector's component; otherwise, False.</returns>
		public static bool operator<(DVector3 u, DVector3 v)
		{
			return (
				(u.mX < v.mX) && 
				(u.mY < v.mY) && 
				(u.mZ < v.mZ));
		}
		/// <summary>
		/// Tests if a vector's components are greater or equal than another vector's components.
		/// </summary>
		/// <param name="u">The left-hand vector.</param>
		/// <param name="v">The right-hand vector.</param>
		/// <returns>True if the left-hand vector's components are greater or equal than the right-hand vector's component; otherwise, False.</returns>
		public static bool operator>=(DVector3 u, DVector3 v)
		{
			return (
				(u.mX >= v.mX) && 
				(u.mY >= v.mY) && 
				(u.mZ >= v.mZ));
		}
		/// <summary>
		/// Tests if a vector's components are smaller or equal than another vector's components.
		/// </summary>
		/// <param name="u">The left-hand vector.</param>
		/// <param name="v">The right-hand vector.</param>
		/// <returns>True if the left-hand vector's components are smaller or equal than the right-hand vector's component; otherwise, False.</returns>
		public static bool operator<=(DVector3 u, DVector3 v)
		{
			return (
				(u.mX <= v.mX) && 
				(u.mY <= v.mY) && 
				(u.mZ <= v.mZ));
		}
		#endregion

		#region Unary Operators
		/// <summary>
		/// Negates the values of the vector.
		/// </summary>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <returns>A new <see cref="DVector3"/> instance containing the negated values.</returns>
		public static DVector3 operator-(DVector3 v)
		{
			return DVector3.Negate(v);
		}
		#endregion

		#region Binary Operators
		/// <summary>
		/// Adds two vectors.
		/// </summary>
		/// <param name="u">A <see cref="DVector3"/> instance.</param>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <returns>A new <see cref="DVector3"/> instance containing the sum.</returns>
		public static DVector3 operator+(DVector3 u, DVector3 v)
		{
			return DVector3.Add(u,v);
		}
		/// <summary>
		/// Adds a vector and a scalar.
		/// </summary>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="DVector3"/> instance containing the sum.</returns>
		public static DVector3 operator+(DVector3 v, double s)
		{
			return DVector3.Add(v,s);
		}
		/// <summary>
		/// Adds a vector and a scalar.
		/// </summary>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="DVector3"/> instance containing the sum.</returns>
		public static DVector3 operator+(double s, DVector3 v)
		{
			return DVector3.Add(v,s);
		}
		/// <summary>
		/// Subtracts a vector from a vector.
		/// </summary>
		/// <param name="u">A <see cref="DVector3"/> instance.</param>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <returns>A new <see cref="DVector3"/> instance containing the difference.</returns>
		/// <remarks>
		///	result[i] = v[i] - w[i].
		/// </remarks>
		public static DVector3 operator-(DVector3 u, DVector3 v)
		{
			return DVector3.Subtract(u,v);
		}
		/// <summary>
		/// Subtracts a scalar from a vector.
		/// </summary>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="DVector3"/> instance containing the difference.</returns>
		/// <remarks>
		/// result[i] = v[i] - s
		/// </remarks>
		public static DVector3 operator-(DVector3 v, double s)
		{
			return DVector3.Subtract(v, s);
		}
		/// <summary>
		/// Subtracts a vector from a scalar.
		/// </summary>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="DVector3"/> instance containing the difference.</returns>
		/// <remarks>
		/// result[i] = s - v[i]
		/// </remarks>
		public static DVector3 operator-(double s, DVector3 v)
		{
			return DVector3.Subtract(s, v);
		}

		/// <summary>
		/// Multiplies a vector by a scalar.
		/// </summary>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="DVector3"/> containing the result.</returns>
		public static DVector3 operator*(DVector3 v, double s)
		{
			return DVector3.Multiply(v,s);
		}
		/// <summary>
		/// Multiplies a vector by a scalar.
		/// </summary>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="DVector3"/> containing the result.</returns>
		public static DVector3 operator*(double s, DVector3 v)
		{
			return DVector3.Multiply(v,s);
		}
		/// <summary>
		/// Divides a vector by a scalar.
		/// </summary>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <returns>A new <see cref="DVector3"/> containing the quotient.</returns>
		/// <remarks>
		/// result[i] = v[i] / s;
		/// </remarks>
		public static DVector3 operator/(DVector3 v, double s)
		{
			return DVector3.Divide(v,s);
		}
		/// <summary>
		/// Divides a scalar by a vector.
		/// </summary>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <returns>A new <see cref="DVector3"/> containing the quotient.</returns>
		/// <remarks>
		/// result[i] = s / v[i]
		/// </remarks>
		public static DVector3 operator/(double s, DVector3 v)
		{
			return DVector3.Divide(s,v);
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
					case 2:
						return mZ;
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
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <returns>An array of double-precision floating point values.</returns>
		public static explicit operator double[](DVector3 v)
		{
			double[] array = new double[3];
			array[0] = v.X;
			array[1] = v.Y;
			array[2] = v.Z;
			return array;
		}
		/// <summary>
		/// Converts the vector to an array of double-precision floating point values.
		/// </summary>
		/// <param name="v">A <see cref="DVector3"/> instance.</param>
		/// <returns>An array of double-precision floating point values.</returns>
        public static explicit operator List<double>(DVector3 v)
		{
            List<double> array = new List<double>(3);
			array[0] = v.X;
			array[1] = v.Y;
			array[2] = v.Z;
			return array;
		}
		#endregion

	}

}
