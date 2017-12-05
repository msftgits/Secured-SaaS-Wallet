﻿using System;
using SecuredCommunication;
using System.Configuration;
using System.Threading;

namespace TransactionEngine
{
    /// <summary>
    /// A sample app that listens for transactions requests.
    /// when one arrives, perform it, and notify about the change 
    /// to the fans
    /// </summary>
    class Program
    {
        #region private members

        private const string c_keyVaultUri = "https://eladiw-testkv.vault.azure.net/";
        private const string c_encKeyName = "enc_public";
        private const string c_decKeyName = "dec_private";
        private const string c_signKeyName = "sign_private";
        private const string c_verifyKeyName = "verify_public";
        private const string c_password = "12345678";

        #endregion

        static void Main(string[] args)
        {
            Console.WriteLine("TransactionEngine - I do as I told");

            // Init
            var unitConverion = new Nethereum.Util.UnitConversion();
            var service = new Nethereum.KeyStore.KeyStoreService();

            var kvInfo = new KeyVault(c_keyVaultUri);
            var secretsMgmnt = new SecretsManagement(c_encKeyName, c_decKeyName, c_signKeyName, c_verifyKeyName, kvInfo, kvInfo);
            var uri = new Uri(ConfigurationManager.AppSettings["rabbitMqUri"]);
            var securedComm = new RabbitMQBusImpl(secretsMgmnt, uri, c_verifyKeyName, c_signKeyName, false, c_encKeyName, c_decKeyName);

            var ethereumNodeWrapper = new EthereumNodeWrapper(kvInfo, secretsMgmnt);

            // Listen on transactions requests, process them and notify the users when done
            var consumerTag =
                securedComm.Dequeue("transactions",
                                          (msg) =>
                                          {
                                              Console.WriteLine("Got work!");

                                               var msgArray = msg.data.Split(";");
                                              var amount = unitConverion.ToWei(msgArray[0]);
                                              var senderName = msgArray[1];
                                              var reciverAddress = msgArray[2];

                                              try
                                              {
                                                  var transactionHash = ethereumNodeWrapper.SignTransaction("sender", reciverAddress, amount).Result;
                                                  var trnsactionResult = ethereumNodeWrapper.SendTransaction(transactionHash).Result;
                                              }
                                              catch (Exception ex)
                                              {
                                                  Console.WriteLine(ex.Message);
                                                  throw;
                                              }

                                              // Wait for miner
                                              Thread.Sleep(30000);

                                              // notify a user about his balance change
                                              securedComm.EnqueueAsync(
                                                "notifications",
                                                new Message(reciverAddress)).Wait();
                                          });
        }
    }
}
