using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;

namespace dm.KAE.Data.Models
{
    public class Holder
    {
        [JsonIgnore]
        public int HolderId { get; set; }
        public string Address { get; set; }
        public string Value { get; set; }
        public string LastBlockNumber { get; set; }
        public DateTimeOffset TimeStamp { get; set; }

        [JsonIgnore]
        public virtual BigInteger ValueBigInt {
            get {
                return BigInteger.Parse(Value);
            }
        }
    }
}
