﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.Extensibility {

    public class UpperCaseTransformer : ValueTransformer {

        public override string Key {
            get { return "toUpperCase"; }
        }

        public override string Name {
            get { return "Convert to upper case"; }
        }

        public override string Description {
            get { return "Converts all characters in input value to upper case"; }
        }

        public override string Transform(string value, IRowSource row) {
            if (!string.IsNullOrEmpty(value)) {
                return value.ToUpper();
            }
            return value;
        }

        public override bool HasOptions {
            get { return false; }
        }

        public override void ShowOptions() {
            throw new NotImplementedException();
        }

        public override object GetConfiguration() {
            return new UpperCaseTransformerConfig { };
        }

        public override void RestoreFromConfiguration(object config) {
            var c = config as UpperCaseTransformerConfig;
        }
    }

    public class UpperCaseTransformerConfig {        
    }
}
