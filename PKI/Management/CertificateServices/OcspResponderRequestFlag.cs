﻿using System;

namespace SysadminsLV.PKI.Management.CertificateServices {
    /// <summary>
    /// Defines a set of flags that controls how the OCSP requests are processed on a Microsoft Online Responder.
    /// <para>This enumeration has a <see cref="FlagsAttribute"/> attribute that allows a bitwise combination of its member values.</para>
    /// </summary>
    [Flags]
    public enum OcspResponderRequestFlag {
        /// <summary>
        /// None.
        /// </summary>
        None            = 0,
        /// <summary>
        /// Instructs Online Responder to reject OCSP requests that have signatures on them.
        /// </summary>
        RejectSignature = 1
    }
}