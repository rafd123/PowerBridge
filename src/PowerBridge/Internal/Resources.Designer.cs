﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PowerBridge.Internal {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PowerBridge.Internal.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to at {0}: line {1}.
        /// </summary>
        internal static string CustomFileLineNumberFormat {
            get {
                return ResourceManager.GetString("CustomFileLineNumberFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You cannot specify both the Expression and File parameters simultaneously..
        /// </summary>
        internal static string ExpressionAndFileParametersCannotBeUsedSimultaneously {
            get {
                return ResourceManager.GetString("ExpressionAndFileParametersCannotBeUsedSimultaneously", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Either the Expression or File parameter must be specified..
        /// </summary>
        internal static string ExpressionOrFileParameterMustBeSpecified {
            get {
                return ResourceManager.GetString("ExpressionOrFileParameterMustBeSpecified", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Windows PowerShell is in non-interactive mode. Read and Prompt functionality is not available..
        /// </summary>
        internal static string PowerShellPromptNotAvailable {
            get {
                return ResourceManager.GetString("PowerShellPromptNotAvailable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The argument &apos;{0}&apos; to the File parameter does not exist. Provide the path to an existing &apos;.ps1&apos; file as an argument to the File parameter..
        /// </summary>
        internal static string PowerShellScriptFileDoesNotExistFormat {
            get {
                return ResourceManager.GetString("PowerShellScriptFileDoesNotExistFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Processing File &apos;{0}&apos; failed because the file does not have a &apos;.ps1&apos; extension. Specify a valid Windows PowerShell script file name, and then try again..
        /// </summary>
        internal static string PowerShellScriptFileMustBeSpecifiedFormat {
            get {
                return ResourceManager.GetString("PowerShellScriptFileMustBeSpecifiedFormat", resourceCulture);
            }
        }
    }
}