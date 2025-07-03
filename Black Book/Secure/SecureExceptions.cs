/// INCORRIGO SYX DIGITAL COMMUNICATION SYSTEMS
// Secure Exceptions - Exceptions for secure operations in Black Book
using System;

namespace BlackBook.Security;
    /// <summary>Thrown when the bundle unwrap fails because of a bad password.</summary>
    public class ProfileAuthenticationException : Exception {
        public ProfileAuthenticationException (string message, Exception? inner = null)
            : base(message, inner) { }
    }

    /// <summary>Thrown when the profile data decrypt fails because of tampering.</summary>
    public class ProfileDecryptionException : Exception {
        public ProfileDecryptionException (string message, Exception? inner = null)
            : base(message, inner) { }
    }

