using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace dm.KAE.Data.Models
{
    public class LastMessage
    {
        public int LastMessageId { get; set; }
        public int MessageId { get; set; }
        public long ChatId { get; set; }
    }
}
