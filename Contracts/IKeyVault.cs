﻿using System.Threading.Tasks;
using Microsoft.Azure.KeyVault.Models;

namespace Contracts
{
    /// <summary>
    /// A wrapper that handles key vault functionality
    /// </summary>
    public interface IKeyVault
    {
        /// <summary>
        /// Gets the specified secret
        /// </summary>
        /// <returns>The secret</returns>
        /// <param name="secretName">Secret identifier</param>
        Task<SecretBundle> GetSecretAsync(string secretName);

        /// <summary>
        /// Sets a secret in keyvault
        /// </summary>
        /// <returns>The secret.</returns>
        /// <param name="secretName">Secret identifier.</param>
        /// <param name="value">The value to be stored.</param>
        Task<SecretBundle> SetSecretAsync(string secretName, string value);
    }
}
