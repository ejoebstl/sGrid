﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.296
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace sGridServer.Resources.AdminDashboard {
    using System;
    
    
    /// <summary>
    ///   Eine stark typisierte Ressourcenklasse zum Suchen von lokalisierten Zeichenfolgen usw.
    /// </summary>
    // Diese Klasse wurde von der StronglyTypedResourceBuilder automatisch generiert
    // -Klasse über ein Tool wie ResGen oder Visual Studio automatisch generiert.
    // Um einen Member hinzuzufügen oder zu entfernen, bearbeiten Sie die .ResX-Datei und führen dann ResGen
    // mit der /str-Option erneut aus, oder Sie erstellen Ihr VS-Projekt neu.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class PerformanceOverview {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal PerformanceOverview() {
        }
        
        /// <summary>
        ///   Gibt die zwischengespeicherte ResourceManager-Instanz zurück, die von dieser Klasse verwendet wird.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("sGridServer.Views.AdminDashboard.Resources.PerformanceOverview", typeof(PerformanceOverview).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Überschreibt die CurrentUICulture-Eigenschaft des aktuellen Threads für alle
        ///   Ressourcenzuordnungen, die diese stark typisierte Ressourcenklasse verwenden.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Count of the devices ähnelt.
        /// </summary>
        public static string DeviceCount {
            get {
                return ResourceManager.GetString("DeviceCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Last result submitted ähnelt.
        /// </summary>
        public static string LastResultTime {
            get {
                return ResourceManager.GetString("LastResultTime", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die No results submitted ähnelt.
        /// </summary>
        public static string NoResults {
            get {
                return ResourceManager.GetString("NoResults", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Count of the results ähnelt.
        /// </summary>
        public static string ResultCount {
            get {
                return ResourceManager.GetString("ResultCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Total Runtime ähnelt.
        /// </summary>
        public static string TotalRuntime {
            get {
                return ResourceManager.GetString("TotalRuntime", resourceCulture);
            }
        }
    }
}
