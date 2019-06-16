using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.Parsers.Models.SqlMap
{
  //  <xs:element name = "result" >
  //  < xs:complexType>
  //    <xs:attribute name = "property" type="xs:string" use="required" />
  //    <xs:attribute name = "column" type="xs:string" />
  //    <xs:attribute name = "lazyLoad" >
  //      < xs:simpleType>
  //        <xs:restriction base="xs:NMTOKEN">
  //          <xs:enumeration value = "false" />
  //          < xs:enumeration value = "true" />
 
  //         </ xs:restriction>
  //      </xs:simpleType>
  //    </xs:attribute>
  //    <xs:attribute name = "select" type="xs:string" />
  //    <xs:attribute name = "nullValue" type="xs:string" />
  //    <xs:attribute name = "type" type="xs:string" />
  //    <xs:attribute name = "dbType" type="xs:string" />
  //    <xs:attribute name = "columnIndex" type="xs:string" />
  //    <xs:attribute name = "resultMapping" type="xs:string" />
  //    <xs:attribute name = "typeHandler" type="xs:string" />
  //  </xs:complexType>
  //</xs:element>
    public class ResultProperties
    {
        public string Name {get; set; }
        public string Column { get; set; }
        public bool LazyLoad { get; set; }
        public string Select { get; set; }
        public string NullValue { get; set; }
        public string Type { get; set; }
        public string DbType { get; set; }
        public string ColumnIndex { get; set; }
        public string ResultMapping { get; set; }
        public string TypeHandler { get; set; }
    }
}
