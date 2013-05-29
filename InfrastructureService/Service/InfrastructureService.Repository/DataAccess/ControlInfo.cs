//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace InfrastructureService.Repository.DataAccess
{
    using System;
    using System.Collections.Generic;
    
    public partial class ControlInfo
    {
        public ControlInfo()
        {
            this.RoleControlMaps = new HashSet<RoleControlMap>();
            this.UserControlMaps = new HashSet<UserControlMap>();
        }
    
        public System.Guid RowID { get; set; }
        public System.Guid ComponentID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public int Sort { get; set; }
        public int Status { get; set; }
    
        public virtual ComponentInfo ComponentInfo { get; set; }
        public virtual ICollection<RoleControlMap> RoleControlMaps { get; set; }
        public virtual ICollection<UserControlMap> UserControlMaps { get; set; }
    }
}
