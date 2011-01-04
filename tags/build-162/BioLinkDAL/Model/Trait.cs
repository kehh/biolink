﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class Trait : GUIDObject{

        public int TraitID { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public string Comment { get; set; }

        public int IntraCatID { get; set; }        

        public string DataType { get; set; }

        public string Validation { get; set; }

        public string Category { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.TraitID; }
        }

    }

    public class TraitCategory : GUIDObject {

        public int TraitCategoryID { get; set; }

        public string Category { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.TraitCategoryID; }
        }

    }

    public class TraitType : GUIDObject {
        public int TraitTypeID { get; set; }
        public string TraitTypeName { get; set; }
        public string DataType { get; set; }
        public string ValidationStr { get; set; }
        public int CategoryID { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.TraitTypeID; }
        }
    }

}
