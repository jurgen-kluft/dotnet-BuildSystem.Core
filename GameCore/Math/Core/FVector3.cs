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
	public enum EAxis
	{
		X = 0,
		Y = 1,
		Z = 2,
		W = 3,
	}

	/// <summary>
	/// Represents 3-Dimentional vector of single-precision floating point numbers.
	/// </summary>
	[Serializable]
	[TypeConverter(typeof(ExpandableObjectConverter))]
	[StructLayout(LayoutKind.Sequential)]
	public struct FVector3 : ISerializable, ICloneable
	{
		#region Private fields
		internal float mX;
		internal float mY;
		internal float mZ;
		#endregion
		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="FVector3"/> class with the specified coordinates.
		/// </summary>
		/// <param name="x">The vector's X coordinate.</param>
		/// <param name="y">The vector's Y coordinate.</param>
		/// <param name="z">The vector's Z coordinate.</param>
		public FVector3(float x, float y, float z)
		{
			mX = x;
			mY = y;
			mZ = z;
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="FVector3"/> class with the specified coordinates.
		/// </summary>
		/// <param name="coordinates">An array containing the coordinate parameters.</param>
		public FVector3(float[] coordinates)
		{
			Debug.Assert(coordinates != null);
			Debug.Assert(coordinates.Length >= 3);

			mX = coordinates[0];
			mY = coordinates[1];
			mZ = coordinates[2];
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="FVector3"/> class with the specified coordinates.
		/// </summary>
		/// <param name="coordinates">An array containing the coordinate parameters.</param>
        public FVector3(List<float> coordinates)
		{
			Debug.Assert(coordinates != null);
			Debug.Assert(coordinates.Count >= 3);

			mX = coordinates[0];
			mY = coordinates[1];
			mZ = coordinates[2];
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="FVector3"/> class using coordinates from a given <see cref="FVector3"/> instance.
		/// </summary>
		/// <param name="vector">A <see cref="FVector3"/> to get the coordinates from.</param>
		public FVector3(FVector3 vector) : this(vector.mX, vector.mY, vector.mZ)
		{
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="FVector3"/> class with serialized data.
		/// </summary>
		/// <param name="info">The object that holds the serialized object data.</param>
		/// <param name="context">The contextual information about the source or destination.</param>
		private FVector3(SerializationInfo info, StreamingContext context)
		{
			mX = info.GetSingle("X");
			mY = info.GetSingle("Y");
			mZ = info.GetSingle("Z");
		}
		#endregion
		#region Set

		public void Set(float inValue)
		{
			mX = inValue;
			mY = inValue;
			mZ = inValue;
		}

		public void Set(float inX, float inY, float inZ)
		{
			mX = inX;
			mY = inY;
			mZ = inZ;
		}

		public void Set(FVector3 inVector)
		{
			mX = inVector.X;
			mY = inVector.Y;
			mZ = inVector.Z;
		}

		#endregion
		#region Constants
		/// <summary>
		/// 3-Dimentional single-precision floating point zero vector.
		/// </summary>
		public static readonly FVector3 sZero = new FVector3(0.0f, 0.0f, 0.0f);
		public static readonly FVector3 sOne = new FVector3(1.0f, 1.0f, 1.0f);
		/// <summary>
		/// 3-Dimentional single-precision floating point X-Axis vector.
		/// </summary>
		public static readonly FVector3 sAxisX	= new FVector3(1.0f, 0.0f, 0.0f);
		/// <summary>
		/// 3-Dimentional single-precision floating point Y-Axis vector.
		/// </summary>
		public static readonly FVector3 sAxisY	= new FVector3(0.0f, 1.0f, 0.0f);
		/// <summary>
		/// 3-Dimentional single-precision floating point Y-Axis vector.
		/// </summary>
		public static readonly FVector3 sAxisZ	= new FVector3(0.0f, 0.0f, 1.0f);
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
		#endregion
		#region ICloneable Members
		/// <summary>
		/// Creates an exact copy of this <see cref="FVector3"/> object.
		/// </summary>
		/// <returns>The <see cref="FVector3"/> object this method creates, cast as an object.</returns>
		object ICloneable.Clone()
		{
			return new FVector3(this);
		}
		/// <summary>
		/// Creates an exact copy of this <see cref="FVector3"/> object.
		/// </summary>
		/// <returns>The <see cref="FVector3"/> object this method creates.</returns>
		public FVector3 Clone()
		{
			return new FVector3(this);
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
		/// Converts the specified string to its <see cref="FVector3"/> equivalent.
		/// </summary>
		/// <param name="s">A string representation of a <see cref="FVector3"/></param>
		/// <returns>A <see cref="FVector3"/> that represents the vector specified by the <paramref name="s"/> parameter.</returns>
		public static FVector3 Parse(string s)
		{
			Regex r = new Regex(@"\((?<x>.*),(?<y>.*),(?<z>.*)\)", RegexOptions.None);
			Match m = r.Match(s);
			if (m.Success)
			{
				return new FVector3(
					float.Parse(m.Result("${x}")),
					float.Parse(m.Result("${y}")),
					float.Parse(m.Result("${z}"))
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
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <param name="w">A <see cref="FVector3"/> instance.</param>
		/// <returns>A new <see cref="FVector3"/> instance containing the sum.</returns>
		public static FVector3 Add(FVector3 v, FVector3 w)
		{
			return new FVector3(v.X + w.X, v.Y + w.Y, v.Z + w.Z);
		}
		/// <summary>
		/// Adds a vector and a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector3"/> instance containing the sum.</returns>
		public static FVector3 Add(FVector3 v, float s)
		{
			return new FVector3(v.X + s, v.Y + s, v.Z + s);
		}
		/// <summary>
		/// Adds two vectors and put the result in the third vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector3"/> instance.</param>
		/// <param name="v">A <see cref="FVector3"/> instance</param>
		/// <param name="w">A <see cref="FVector3"/> instance to hold the result.</param>
		public static void Add(FVector3 u, FVector3 v, FVector3 w)
		{
			w.X = u.X + v.X;
			w.Y = u.Y + v.Y;
			w.Z = u.Z + v.Z;
		}
		/// <summary>
		/// Adds a vector and a scalar and put the result into another vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <param name="v">A <see cref="FVector3"/> instance to hold the result.</param>
		public static void Add(FVector3 u, float s, FVector3 v)
		{
			v.X = u.X + s;
			v.Y = u.Y + s;
			v.Z = u.Z + s;
		}
		/// <summary>
		/// Subtracts a vector from a vector.
		/// </summary>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <param name="w">A <see cref="FVector3"/> instance.</param>
		/// <returns>A new <see cref="FVector3"/> instance containing the difference.</returns>
		/// <remarks>
		///	result[i] = v[i] - w[i].
		/// </remarks>
		public static FVector3 Subtract(FVector3 v, FVector3 w)
		{
			return new FVector3(v.X - w.X, v.Y - w.Y, v.Z - w.Z);
		}
		/// <summary>
		/// Subtracts a scalar from a vector.
		/// </summary>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector3"/> instance containing the difference.</returns>
		/// <remarks>
		/// result[i] = v[i] - s
		/// </remarks>
		public static FVector3 Subtract(FVector3 v, float s)
		{
			return new FVector3(v.X - s, v.Y - s, v.Z - s);
		}
		/// <summary>
		/// Subtracts a vector from a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector3"/> instance containing the difference.</returns>
		/// <remarks>
		/// result[i] = s - v[i]
		/// </remarks>
		public static FVector3 Subtract(float s, FVector3 v)
		{
			return new FVector3(s - v.X, s - v.Y, s - v.Z);
		}
		/// <summary>
		/// Subtracts a vector from a second vector and puts the result into a third vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector3"/> instance.</param>
		/// <param name="v">A <see cref="FVector3"/> instance</param>
		/// <param name="w">A <see cref="FVector3"/> instance to hold the result.</param>
		/// <remarks>
		///	w[i] = v[i] - w[i].
		/// </remarks>
		public static void Subtract(FVector3 u, FVector3 v, FVector3 w)
		{
			w.X = u.X - v.X;
			w.Y = u.Y - v.Y;
			w.Z = u.Z - v.Z;
		}
		/// <summary>
		/// Subtracts a vector from a scalar and put the result into another vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <param name="v">A <see cref="FVector3"/> instance to hold the result.</param>
		/// <remarks>
		/// v[i] = u[i] - s
		/// </remarks>
		public static void Subtract(FVector3 u, float s, FVector3 v)
		{
			v.X = u.X - s;
			v.Y = u.Y - s;
			v.Z = u.Z - s;
		}
		/// <summary>
		/// Subtracts a scalar from a vector and put the result into another vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <param name="v">A <see cref="FVector3"/> instance to hold the result.</param>
		/// <remarks>
		/// v[i] = s - u[i]
		/// </remarks>
		public static void Subtract(float s, FVector3 u, FVector3 v)
		{
			v.X = s - u.X;
			v.Y = s - u.Y;
			v.Z = s - u.Z;
		}
		/// <summary>
		/// Divides a vector by another vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector3"/> instance.</param>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <returns>A new <see cref="FVector3"/> containing the quotient.</returns>
		/// <remarks>
		///	result[i] = u[i] / v[i].
		/// </remarks>
		public static FVector3 Divide(FVector3 u, FVector3 v)
		{
			return new FVector3(u.X / v.X, u.Y / v.Y, u.Z / v.Z);
		}
		/// <summary>
		/// Divides a vector by a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <returns>A new <see cref="FVector3"/> containing the quotient.</returns>
		/// <remarks>
		/// result[i] = v[i] / s;
		/// </remarks>
		public static FVector3 Divide(FVector3 v, float s)
		{
			return new FVector3(v.X / s, v.Y / s, v.Z / s);
		}
		/// <summary>
		/// Divides a scalar by a vector.
		/// </summary>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <returns>A new <see cref="FVector3"/> containing the quotient.</returns>
		/// <remarks>
		/// result[i] = s / v[i]
		/// </remarks>
		public static FVector3 Divide(float s, FVector3 v)
		{
			return new FVector3(s / v.X, s/ v.Y, s / v.Z);
		}
		/// <summary>
		/// Divides a vector by another vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector3"/> instance.</param>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <param name="w">A <see cref="FVector3"/> instance to hold the result.</param>
		/// <remarks>
		/// w[i] = u[i] / v[i]
		/// </remarks>
		public static void Divide(FVector3 u, FVector3 v, FVector3 w)
		{
			w.X = u.X / v.X;
			w.Y = u.Y / v.Y;
			w.Z = u.Z / v.Z;
		}
		/// <summary>
		/// Divides a vector by a scalar.
		/// </summary>
		/// <param name="u">A <see cref="FVector3"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <param name="v">A <see cref="FVector3"/> instance to hold the result.</param>
		/// <remarks>
		/// v[i] = u[i] / s
		/// </remarks>
		public static void Divide(FVector3 u, float s, FVector3 v)
		{
			v.X = u.X / s;
			v.Y = u.Y / s;
			v.Z = u.Z / s;
		}
		/// <summary>
		/// Divides a scalar by a vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector3"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <param name="v">A <see cref="FVector3"/> instance to hold the result.</param>
		/// <remarks>
		/// v[i] = s / u[i]
		/// </remarks>
		public static void Divide(float s, FVector3 u, FVector3 v)
		{
			v.X = s / u.X;
			v.Y = s / u.Y;
			v.Z = s / u.Z;
		}
		/// <summary>
		/// Multiplies a vector by a scalar.
		/// </summary>
		/// <param name="u">A <see cref="FVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector3"/> containing the result.</returns>
		public static FVector3 Multiply(FVector3 u, float s)
		{
			return new FVector3(u.X * s, u.Y * s, u.Z * s);
		}
		/// <summary>
		/// Multiplies a vector by a scalar and put the result in another vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <param name="v">A <see cref="FVector3"/> instance to hold the result.</param>
		public static void Multiply(FVector3 u, float s, FVector3 v)
		{
			v.X = u.X * s;
			v.Y = u.Y * s;
			v.Z = u.Z * s;
		}
		/// <summary>
		/// Calculates the dot product of two vectors.
		/// </summary>
		/// <param name="u">A <see cref="FVector3"/> instance.</param>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <returns>The dot product value.</returns>
		public static float DotProduct(FVector3 u, FVector3 v)
		{
			return (u.X * v.X) + (u.Y * v.Y) + (u.Z * v.Z);
		}
		public static float DotProduct(FVector3 u, float x, float y, float z)
		{
			return (u.X * x) + (u.Y * y) + (u.Z * z);
		}
		/// <summary>
		/// Calculates the cross product of two vectors.
		/// </summary>
		/// <param name="u">A <see cref="FVector3"/> instance.</param>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <returns>A new <see cref="FVector3"/> containing the cross product result.</returns>
		public static FVector3 CrossProduct(FVector3 u, FVector3 v)
		{
			return new FVector3(
				u.mY * v.mZ - u.mZ * v.mY,
				u.mZ * v.mX - u.mX * v.mZ,
				u.mX * v.mY - u.mY * v.mX);
		}
		public static FVector3 CrossProduct(FVector4 u, FVector4 v)
		{
			return new FVector3(
				u.mY * v.mZ - u.mZ * v.mY,
				u.mZ * v.mX - u.mX * v.mZ,
				u.mX * v.mY - u.mY * v.mX);
		}
		/// <summary>
		/// Calculates the cross product of two vectors.
		/// </summary>
		/// <param name="u">A <see cref="FVector3"/> instance.</param>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <param name="w">A <see cref="FVector3"/> instance to hold the cross product result.</param>
		public static void CrossProduct(FVector3 u, FVector3 v, FVector3 w)
		{
			w.X = u.Y*v.Z - u.Z*v.Y;
			w.Y = u.Z*v.X - u.X*v.Z;
			w.Z = u.X*v.Y - u.Y*v.X;
		}
		/// <summary>
		/// Negates a vector.
		/// </summary>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <returns>A new <see cref="FVector3"/> instance containing the negated values.</returns>
		public static FVector3 Negate(FVector3 v)
		{
			return new FVector3(-v.X, -v.Y, -v.Z);
		}
		/// <summary>
		/// Tests whether two vectors are approximately equal using default tolerance value.
		/// </summary>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <param name="u">A <see cref="FVector3"/> instance.</param>
		/// <returns>True if the two vectors are approximately equal; otherwise, False.</returns>
		public static bool ApproxEqual(FVector3 v, FVector3 u)
		{
            return ApproxEqual(v, u, Math.Epsilon);
		}
		/// <summary>
		/// Tests whether two vectors are approximately equal given a tolerance value.
		/// </summary>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <param name="u">A <see cref="FVector3"/> instance.</param>
		/// <param name="tolerance">The tolerance value used to test approximate equality.</param>
		/// <returns>True if the two vectors are approximately equal; otherwise, False.</returns>
		public static bool ApproxEqual(FVector3 v, FVector3 u, float tolerance)
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
		/// Returns least significant axis
		/// </summary>
		/// <returns></returns>
		public EAxis GetLeastSignificantAxis()
		{ 
			int s = 0; 
			for (int c=1; c<3; c++)
				if (Math.Abs(this[c]) < Math.Abs(this[s]))
					s = c; 
			return (EAxis)s; 
		}
		/// <summary>
		/// Returns least significant axis
		/// </summary>
		/// <returns></returns>
		public EAxis GetMostSignificantAxis()
		{ 
			int s = 0; 
			for (int c=1; c<3; c++)
				if (Math.Abs(this[c]) > Math.Abs(this[s])) 
					s = c;
			return (EAxis)s;
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
		}
		public FVector3 Normalized()
		{
			float length = Length();
			if (length == 0)
			{
				throw new DivideByZeroException("Trying to normalize a vector with length of zero.");
			}

			FVector3 vector = new FVector3();
			vector.mX = mX / length;
			vector.mY = mY / length;
			vector.mZ = mZ / length;
			return vector;
		}
		/// <summary>
		/// Get vector that is perpendicular to this one (a random one in the plane) 
		/// </summary>
		/// <returns></returns>
		public FVector3 GetNormalizedPerpendicular()
		{
			return Math.Abs(mZ) > Math.Abs(mY) ? new FVector3(-mZ, 0.0f, mX).Normalized() : new FVector3(-mY, mX, 0.0f).Normalized();
		}
		/// <summary>
		/// Returns the length of the vector.
		/// </summary>
		/// <returns>The length of the vector. (Sqrt(X*X + Y*Y + Z*Z))</returns>
		public float Length()
		{
			return (float)System.Math.Sqrt(mX*mX + mY*mY + mZ*mZ);
		}
		/// <summary>
		/// Returns the squared length of the vector.
		/// </summary>
		/// <returns>The squared length of the vector. (X*X + Y*Y + Z*Z)</returns>
		public float SquareLength()
		{
			return (mX*mX + mY*mY + mZ*mZ);
		}
		/// <summary>
		/// Return True if they are almost the same
		/// </summary>
		/// <param name="inVector"></param>
		/// <returns>True/False, True if the vectors are almost the same</returns>
		public bool IsClose(FVector3 inVector)
		{
            return Math.IsNear(mX, inVector.mX, 1e-05f) && Math.IsNear(mY, inVector.mY, 1e-05f) && Math.IsNear(mZ, inVector.mZ, 1e-05f);
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
		/// <returns>True if <paramref name="obj"/> is a <see cref="FVector3"/> and has the same values as this instance; otherwise, False.</returns>
		public override bool Equals(object obj)
		{
			if (obj is FVector3)
			{
				FVector3 v = (FVector3)obj;
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
		public static bool operator==(FVector3 u, FVector3 v)
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
		public static bool operator!=(FVector3 u, FVector3 v)
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
		public static bool operator>(FVector3 u, FVector3 v)
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
		public static bool operator<(FVector3 u, FVector3 v)
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
		public static bool operator>=(FVector3 u, FVector3 v)
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
		public static bool operator<=(FVector3 u, FVector3 v)
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
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <returns>A new <see cref="FVector3"/> instance containing the negated values.</returns>
		public static FVector3 operator-(FVector3 v)
		{
			return FVector3.Negate(v);
		}
		#endregion
		#region Binary Operators
		/// <summary>
		/// Adds two vectors.
		/// </summary>
		/// <param name="u">A <see cref="FVector3"/> instance.</param>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <returns>A new <see cref="FVector3"/> instance containing the sum.</returns>
		public static FVector3 operator+(FVector3 u, FVector3 v)
		{
			return FVector3.Add(u,v);
		}
		/// <summary>
		/// Adds a vector and a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector3"/> instance containing the sum.</returns>
		public static FVector3 operator+(FVector3 v, float s)
		{
			return FVector3.Add(v,s);
		}
		/// <summary>
		/// Adds a vector and a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector3"/> instance containing the sum.</returns>
		public static FVector3 operator+(float s, FVector3 v)
		{
			return FVector3.Add(v,s);
		}
		/// <summary>
		/// Subtracts a vector from a vector.
		/// </summary>
		/// <param name="u">A <see cref="FVector3"/> instance.</param>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <returns>A new <see cref="FVector3"/> instance containing the difference.</returns>
		/// <remarks>
		///	result[i] = v[i] - w[i].
		/// </remarks>
		public static FVector3 operator-(FVector3 u, FVector3 v)
		{
			return FVector3.Subtract(u,v);
		}
		/// <summary>
		/// Subtracts a scalar from a vector.
		/// </summary>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector3"/> instance containing the difference.</returns>
		/// <remarks>
		/// result[i] = v[i] - s
		/// </remarks>
		public static FVector3 operator-(FVector3 v, float s)
		{
			return FVector3.Subtract(v, s);
		}
		/// <summary>
		/// Subtracts a vector from a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector3"/> instance containing the difference.</returns>
		/// <remarks>
		/// result[i] = s - v[i]
		/// </remarks>
		public static FVector3 operator-(float s, FVector3 v)
		{
			return FVector3.Subtract(s, v);
		}

		/// <summary>
		/// Multiplies a vector by a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector3"/> containing the result.</returns>
		public static FVector3 operator*(FVector3 v, float s)
		{
			return FVector3.Multiply(v,s);
		}
		/// <summary>
		/// Multiplies a vector by a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <param name="s">A scalar.</param>
		/// <returns>A new <see cref="FVector3"/> containing the result.</returns>
		public static FVector3 operator*(float s, FVector3 v)
		{
			return FVector3.Multiply(v,s);
		}
		/// <summary>
		/// Divides a vector by a scalar.
		/// </summary>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <returns>A new <see cref="FVector3"/> containing the quotient.</returns>
		/// <remarks>
		/// result[i] = v[i] / s;
		/// </remarks>
		public static FVector3 operator/(FVector3 v, float s)
		{
			return FVector3.Divide(v,s);
		}
		/// <summary>
		/// Divides a scalar by a vector.
		/// </summary>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <param name="s">A scalar</param>
		/// <returns>A new <see cref="FVector3"/> containing the quotient.</returns>
		/// <remarks>
		/// result[i] = s / v[i]
		/// </remarks>
		public static FVector3 operator/(float s, FVector3 v)
		{
			return FVector3.Divide(s,v);
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
		/// Converts the vector to an array of single-precision floating point values.
		/// </summary>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <returns>An array of single-precision floating point values.</returns>
		public static explicit operator float[](FVector3 v)
		{
			float[] array = new float[3];
			array[0] = v.X;
			array[1] = v.Y;
			array[2] = v.Z;
			return array;
		}
		/// <summary>
		/// Converts the vector to an array of single-precision floating point values.
		/// </summary>
		/// <param name="v">A <see cref="FVector3"/> instance.</param>
		/// <returns>An array of single-precision floating point values.</returns>
        public static explicit operator List<float>(FVector3 v)
		{
            List<float> array = new List<float>(3);
			array[0] = v.X;
			array[1] = v.Y;
			array[2] = v.Z;
			return array;
		}
		#endregion
	}

}
