using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Linq;

namespace NoIIS
{
	/// <summary>
	/// This class is an implementation of the HttpFileCollectionBase class from System.Web.
	/// </summary>
	public class NoIISFileCollection : HttpFileCollectionBase
	{
		private List<NoIISPostedFile> files = new List<NoIISPostedFile>();
		
		public NoIISFileCollection() : base()
		{
		}
		
		public NoIISFileCollection(IList<NoIISPostedFile> files) : base()
		{
			this.files.AddRange(files);
		}
		
		public override string[] AllKeys
		{
			get
			{
				return this.files.Select(n => n.FormName).ToArray();
			}
		}
		
		public override void CopyTo(Array dest, int index)
		{
			Array.Copy(this.files.ToArray(), 0, dest, index, this.files.Count);
		}
		
		public override int Count
		{
			get
			{
				return this.files.Count;
			}
		}
		
		public override bool Equals(object obj)
		{
			var other = obj as NoIISFileCollection;
			
			if(other == null)
			{
				return false;
			}
			
			if(obj == null)
			{
				return false;
			}
			
			if(this.files.Count != other.files.Count)
			{
				return false;
			}
			
			for(var n = 0; n < this.files.Count; n++)
			{
				var a = this.files[n];
				var b = other.files[n];
				
				if(!a.Equals(b))
				{
					return false;
				}
			}
			
			return true;
		}

		public override int GetHashCode()
		{
			return this.files.GetHashCode();
		}

		public override HttpPostedFileBase Get(int index)
		{
			return this.files[index];
		}
		
		public override HttpPostedFileBase Get(string name)
		{
			return this.files.Where(n => n.FormName == name).FirstOrDefault();
		}
		
		public override System.Collections.IEnumerator GetEnumerator()
		{
			return this.files.GetEnumerator();
		}
		
		public override string GetKey(int index)
		{
			return this.files[index].FormName;
		}
		
		public override IList<HttpPostedFileBase> GetMultiple(string name)
		{
			return (IList<HttpPostedFileBase>)this.files.Where(n => n.FormName == name).Select((n, r) => n as HttpPostedFileBase);
		}
		
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
		}
		
		public override bool IsSynchronized
		{
			get
			{
				return false;
			}
		}
		
		public override KeysCollection Keys
		{
			get
			{
				var tmp = new NameValueCollection();
				foreach(var i in this.files)
				{
					tmp.Add(i.FormName, string.Empty);
				}
				
				return tmp.Keys;
			}
		}
		
		public override void OnDeserialization(object sender)
		{
		}
		
		public override object SyncRoot
		{
			get
			{
				return null;
			}
		}
		
		public override HttpPostedFileBase this[int index]
		{
			get
			{
				return this.files[index];
			}
		}
		
		public override HttpPostedFileBase this[string name]
		{
			get
			{
				return this.files.Where(n => n.FormName == name).FirstOrDefault();
			}
		}
	}
}
