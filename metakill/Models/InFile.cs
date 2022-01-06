using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace metakill.Models
{
    class InFile : BindableBase, IEquatable<InFile>
    {
        private bool selected;
        private string status;
        private string remarks;

        public string FileName { get; set; }
        public bool Selected { get => selected; set => SetProperty(ref selected, value); }
        public string Status { get => status; set => SetProperty(ref status, value); }
        public string Remarks { get => remarks; set => SetProperty(ref remarks, value); }

        public override bool Equals(object obj)
        {
            return Equals(obj as InFile);
        }

        public bool Equals(InFile other)
        {
            return other != null &&
                   FileName == other.FileName;
        }

        public override int GetHashCode()
        {
            return 901043656 + EqualityComparer<string>.Default.GetHashCode(FileName);
        }

        public static bool operator ==(InFile left, InFile right)
        {
            return EqualityComparer<InFile>.Default.Equals(left, right);
        }

        public static bool operator !=(InFile left, InFile right)
        {
            return !(left == right);
        }
    }
}
