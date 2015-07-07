using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime;
using System.Web;
using System.Web.Util;

namespace NoIIS
{
	/// <summary>
	/// This class is implementation of the HttpFileCollectionBase class from System.Web.
	/// </summary>
	public class NoIISFileCollection : HttpFileCollectionBase
	{
		public NoIISFileCollection() : base()
		{
		}
		
		public NoIISFileCollection(IList<NoIISPostedFile> files) : base()
		{
			
		}
		
		public override string[] AllKeys {
			get {
				return base.AllKeys;
			}
		}
		
		public override void CopyTo(Array dest, int index)
		{
			base.CopyTo(dest, index);
		}
		
		public override int Count {
			get {
				return base.Count;
			}
		}
		
		public override bool Equals(object obj)
		{
			NoIISFileCollection other = obj as NoIISFileCollection;
				if (other == null)
					return false;
						return true;
		}

		public override int GetHashCode()
		{
			int hashCode = 0;
			return hashCode;
		}

		public override HttpPostedFileBase Get(int index)
		{
			return base.Get(index);
		}
		
		public override HttpPostedFileBase Get(string name)
		{
			return base.Get(name);
		}
		
		public override System.Collections.IEnumerator GetEnumerator()
		{
			return base.GetEnumerator();
		}
		
		public override string GetKey(int index)
		{
			return base.GetKey(index);
		}
		
		public override IList<HttpPostedFileBase> GetMultiple(string name)
		{
			return base.GetMultiple(name);
		}
		
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			base.GetObjectData(info, context);
		}
		
		public override bool IsSynchronized {
			get {
				return base.IsSynchronized;
			}
		}
		
		public override KeysCollection Keys {
			get {
				return base.Keys;
			}
		}
		
		public override void OnDeserialization(object sender)
		{
			base.OnDeserialization(sender);
		}
		
		public override object SyncRoot {
			get {
				return base.SyncRoot;
			}
		}
		
		public override HttpPostedFileBase this[int index] {
			get {
				return base[index];
			}
		}
		
		public override HttpPostedFileBase this[string name] {
			get {
				return base[name];
			}
		}
	}
}
