﻿using System;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault.Models;
using Nethereum.Web3;
using Wallet.Cryptography;
using EthECKey = Nethereum.Signer.EthECKey;

namespace Wallet.Blockchain
{
    /// <summary>
    /// Class for managing the Ethereum account and sending transactions.
    /// </summary>
    public class EthereumAccount : IBlockchainAccount
    {
        private readonly Web3 m_web3;
        private IKeyVault m_kv;

        #region Public Methods
        public EthereumAccount(IKeyVault keyVault, string nodeUrl = "")
        {
            m_kv = keyVault;
            m_web3 = string.IsNullOrEmpty(nodeUrl) ? new Web3() : new Web3(nodeUrl);
        }

        /// <summary>
        /// Creates blockchain account if needed and store the private key in Azure KeyVault 
        /// </summary>
        /// <param name="identifier">key pair identifier.</param>
        /// <param name="privateKey">The given private key to store, if not supplied a new private key will be generated</param>
        public async Task CreateAccountAsync(string identifier, string privateKey = "")
        {
            if (string.IsNullOrEmpty(privateKey))
            {
                var account = EthECKey.GenerateKey();
                privateKey = account.GetPrivateKey();
            }

            await StoreAccountAsync(identifier, privateKey);
        }

        /// <summary>
        /// Returns the public key by the key vault identifier
        /// </summary>
        /// <param name="identifier">The user id</param>
        /// <returns>The user's public address</returns>
        public async Task<string> GetPublicAddressAsync(string identifier)
        {
            var privatekey = await GetPrivateKeyAsync(identifier);
            return new EthECKey(privatekey).GetPublicAddress();
        }

        /// <summary>
        /// Sign a blockchain transaction
        /// </summary>
        /// <param name="senderIdentifier">The sender identifier (Id, name etc.)</param>
        /// <param name="recieverAddress">The receiver address</param>
        /// <param name="amountInWei">The amount to send in Wei (ethereum units)</param>
        /// <returns>The transaction hash</returns>
        public async Task<string> SignTransactionAsync(string senderIdentifier, string recieverAddress, BigInteger amountInWei)
        {
            var senderPrivateKey = await GetPrivateKeyAsync(senderIdentifier);
            var senderEthKey = new EthECKey(senderPrivateKey);

            var txCount = await m_web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(senderEthKey.GetPublicAddress());
            return Web3.OfflineTransactionSigner.SignTransaction(senderPrivateKey, recieverAddress, amountInWei, txCount.Value);
        }

        /// <summary>
        /// Send the transaction to the public node. 
        /// </summary>
        /// <param name="hash">The transaction hash</param>
        /// <returns>The transaction result</returns>
        public async Task<string> SendRawTransactionAsync(string hash)
        {
            return await m_web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(hash);
        }

        /// <summary>
        /// Gets the balance of the provided account
        /// </summary>
        /// <param name="address">The public address of the account</param>
        /// <returns>Returns the balance in ether.</returns>
        public async Task<decimal> GetCurrentBalance(string address)
        {
            var unitConverion = new Nethereum.Util.UnitConversion();
            return unitConverion.FromWei(await m_web3.Eth.GetBalance.SendRequestAsync(address));
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Stores the account async.
        /// </summary>
        /// <returns>If the account was created successfully</returns>
        /// <param name="identifier">Identifier.</param>
        /// <param name="privateKey">The private key.</param>
        private async Task StoreAccountAsync(string identifier, string privateKey)
        {
            try
            {
                await m_kv.SetSecretAsync(identifier, privateKey);
            }
            catch (KeyVaultErrorException exc)
            {
                Console.WriteLine(exc);
                throw;
            }
        }

        /// <summary>
        /// Returns the private key by the key vault identifier
        /// </summary>
        /// <param name="identifier">The user id</param>
        /// <returns>The user's public key</returns>
        private async Task<string> GetPrivateKeyAsync(string identifier)
        {
            return (await m_kv.GetSecretAsync(identifier)).Value;
        }
        #endregion
    }
}