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

    public class MultimediaLink : OwnedDataObject {

        public int MultimediaID { get; set; }

        public int MultimediaLinkID { get; set; }

        public string MultimediaType { get; set; }

        public string Name { get; set; }

        public string Caption { get; set; }

        public string Extension { get; set; }

        public int SizeInBytes { get; set; }

        public int Changes { get; set; }

        public int BlobChanges { get; set; }

        public string TempFilename { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.MultimediaLinkID; }
        }
    }

    public class MultimediaType {

        public int ID { get; set; }

        public string Name { get; set; }
    }


}
