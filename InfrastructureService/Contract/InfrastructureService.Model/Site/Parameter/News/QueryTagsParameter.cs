﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace InfrastructureService.Model.Site
{
    [DataContract]
    public class QueryTagsParameter : HeaderEntity
    {
        [DataMember]
        public int TakeCount { get; set; }
    }
}