﻿/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class TaxonRank : OwnedDataObject {

        public const string INCERTAE_SEDIS = "IS";
        public const string SPECIES_INQUIRENDA = "SI";

        public const string SPECIES = "SP";

        public string KingdomCode { get; set; }

        public string Code { get; set; }

        public string LongName { get; set; }

        public string TextBeforeInFullName { get; set; }

        public string TextAfterInFullName { get; set; }

        public System.Nullable<bool> JoinToParentInFullName { get; set; }

        public string ChecklistDisplayAs { get; set; }

        public System.Nullable<bool> AvailableNameAllowed { get; set; }

        public System.Nullable<bool> UnplacedAllowed { get; set; }

        public System.Nullable<bool> ChgCombAllowed { get; set; }

        public System.Nullable<bool> LituratueNameAllowed { get; set; }

        public string Category { get; set; }

        public string ValidChildList { get; set; }

        public string ValidEndingList { get; set; }

        public System.Nullable<int> Order { get; set; }

        public override string ToString() {
            return String.Format("TaxonRank[{0}]: {1} ({2}) #{3}", KingdomCode, LongName, Category, Order);
        }

        public String GetElementTypeLongName(Taxon taxon) {
            string longrank = LongName;
            if (taxon.AvailableName.GetValueOrDefault(false)) {
                longrank += " Available Name";
            } else if (taxon.LiteratureName.GetValueOrDefault(false)) {
                longrank += " Literature Name";
            }
            return longrank;
        }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return null; }
        }
        
    }
}
