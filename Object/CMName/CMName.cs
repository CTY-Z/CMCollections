using CMFramework.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMFramework.Core
{
    public struct CMName : IEquatable<CMName>, IComparable<CMName>
    {
        private int _hashID;
        public int hashID => _hashID;

        public static readonly CMName None = new CMName("None");

        public CMName(string name)
        {
            _hashID = CMNameEntry.instance.GetHashFromString(name);
        }

        public override string ToString()
        {
            return CMNameEntry.instance.GetStringFromHash(_hashID);
        }

        public override bool Equals(object obj)
        {
            return obj is CMName name && Equals(name);
        }

        public bool Equals(CMName other)
        {
            return _hashID == other._hashID;
        }

        public override int GetHashCode()
        {
            return _hashID;
        }

        public int CompareTo(CMName other)
        {
            return _hashID.CompareTo(other._hashID);
        }

        public static bool operator ==(CMName a, CMName b) => a._hashID == b._hashID;
        public static bool operator !=(CMName a, CMName b) => a._hashID != b._hashID;

        public static implicit operator CMName(string name) => new CMName(name);
        public static implicit operator string(CMName name) => CMNameEntry.instance.GetStringFromHash(name.hashID);
    }
}
