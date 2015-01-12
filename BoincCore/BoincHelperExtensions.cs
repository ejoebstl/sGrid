// This source file is part of BoincGuiRpc.Net
//
// Author: 	    Emanuel Jöbstl <emi@eex-dev.net>
// Weblink: 	http://boincguirpc.codeplex.com
//
// Licensed under the MIT License
//
// (c) 2012-2013

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net.Sockets;

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// This attribute is required for using extension methods with while targeting .Net framework 2.0. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly |
        AttributeTargets.Method)]
    public sealed class ExtensionAttribute : Attribute { }
}

namespace Boinc
{
    /// <summary>
    /// This class defines Xml writer and reader helper extensions for working with 
    /// the Boinc RPC format. 
    /// </summary>
    static class BoincHelperExtensions
    {
        /// <summary>
        /// The tag of a gui rpc request.
        /// </summary>
        public const string GuiRpcRequestTag = "boinc_gui_rpc_request";

        /// <summary>
        /// The tag of a gui rpc reply. 
        /// </summary>
        public const string GuiRpcReplyTag = "boinc_gui_rpc_reply";

        // Private tags for error and unauthorized
        private const string ErrorTag = "error";
        private const string UnauthorizedTag = "unauthorized";

        /// <summary>
        /// Writes the start tag of a boinc request. 
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        internal static void WriteStartBoincRequest(this XmlWriter writer)
        {
            writer.WriteStartElement(GuiRpcRequestTag);
        }

        /// <summary>
        /// Writes the end tag of a boinc request and flushes afterwards.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        internal static void WriteEndBoincRequest(this XmlWriter writer)
        {
            writer.WriteEndElement();
            writer.Flush();
        }

        /// <summary>
        /// Writes the start tag of the given element.
        /// </summary>
        /// <param name="elementName">The name of the element.</param>
        /// <param name="writer">The writer to write to.</param>
        internal static void WriteStartBoincElement(this XmlWriter writer, string elementName)
        {
            writer.WriteStartElement(elementName);
        }

        /// <summary>
        /// Writes the end tag of the given element.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        internal static void WriteEndBoincElement(this XmlWriter writer)
        {
            writer.WriteEndElement();
        }

        /// <summary>
        /// Reads the start tag of a Boinc reply. 
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        internal static void ReadStartBoincReply(this XmlReader reader)
        {
            reader.ReadBoinc(GuiRpcReplyTag);
        }

        /// <summary>
        /// Reads a Boinc element tag. 
        /// </summary>
        /// <param name="expectedNode">The name of the node to read</param>
        /// <param name="throwOnUnexpected">A bool indicating whether an exception should be thrown when an unexpected tag is found. 
        /// If set to false, the method returns false in case of an error instead of throwing an exception</param>
        /// <param name="reader">The reader to read from.</param>
        /// <returns>A bool indicating success or error.</returns>
        internal static bool ReadBoinc(this XmlReader reader, string expectedNode, bool throwOnUnexpected = true)
        {
            reader.Read();
            System.Diagnostics.Debug.WriteLine("CurrentNode: " + reader.Name);

            //If we read an error tag, something went terribly wrong. 
            if (reader.Name.ToLower() == ErrorTag)
            {
                reader.Read();
                throw new BoincApiException("Boinc error: " + reader.Value);
            } 
            
            //If we read an unauthorized tag, we are not authorized. 
            if (reader.Name.ToLower() == UnauthorizedTag)
            {
                throw new BoincApiException("Boinc authorization error. Incorrect password?");
            }

            //If we read a tag which was not expected, something is ill-formed. 
            if (reader.Name != expectedNode)
            {
                if (throwOnUnexpected)
                {
                    throw new InvalidOperationException("Unexpected node: " + reader.Name + ". Expected: " + expectedNode + ". This Exception usually indicates a programming error. Please contact the developer.");
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Reads the end tag of a Boinc reply. 
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        internal static void ReadEndBoincReply(this XmlReader reader)
        {
            reader.ReadBoinc(GuiRpcReplyTag);
        }

        /// <summary>
        /// Ascii 0x03, End-Of-Text, used to indicate the end of Boinc requests and replies. 
        /// </summary>
        private const byte EndBoincRequestCode = 0x03;

        /// <summary>
        /// Forces the network stream to send out an ASCII 0x03 char (End-Of-Text).
        /// This method must be called after WriteEndBoincRequest has been called. 
        /// </summary>
        /// <param name="stream">The network to end the Boinc request for.</param>
        internal static void EndSendBoincRequest(this NetworkStream stream)
        {
            stream.WriteByte(EndBoincRequestCode);
            stream.Flush();
        }
    }
}
