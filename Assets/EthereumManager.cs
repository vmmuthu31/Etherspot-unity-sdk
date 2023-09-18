using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.KeyStore;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;

public class EthereumManager : MonoBehaviour
{
    public string nodeUrl = "https://avalanche-fuji.infura.io/v3/9adf983e23e74d2cba129f28a81d699a";
    public string accountAddress = "0x01e24d130cf4c5599954115c5026276b4797a171";
    public string contractAddress = "0x703A7c280Ea07749Ba5D72973Bc3B4805915493b";
    public string abi = @"{
                        'inputs': [{'internalType': 'address','name': 'account','type': 'address'}],
                        'name': 'getBalance',
                        'outputs': [{'internalType': 'uint256','name': '','type': 'uint256'}],
                        'stateMutability': 'view',
                        'type': 'function'
                    }";

    async void Start()
    {
        // Initialize Web3
        Web3 web3 = new Web3(nodeUrl);

        // Create new wallet
        Account newAccount = CreateWallet("yourStrongPasswordHere");

        // Log new account info
        Debug.Log($"New account address: {newAccount.Address}");
        Debug.Log($"New account private key: {newAccount.PrivateKey}");

        // Get the ETH balance
        var balance = await web3.Eth.GetBalance.SendRequestAsync(newAccount.Address);
        decimal etherAmount = Web3.Convert.FromWei(balance.Value);

        // Output the balance
        Debug.Log($"Balance of address {newAccount.Address}: {etherAmount} ETH");

        // If you also want to interact with a smart contract to check a token balance
        var contract = web3.Eth.GetContract(abi, contractAddress);
    }

    public Account CreateWallet(string password)
    {
        // Generate a new private key
        var ecKey = Nethereum.Signer.EthECKey.GenerateKey();

        // Create a new account based on the private key
        var newAccount = new Account(ecKey);

        // Generate the keystore file to encrypt the private key
        var keystoreService = new KeyStoreService();
        var keystore = keystoreService.EncryptAndGenerateDefaultKeyStoreAsJson(password, ecKey.GetPrivateKeyAsBytes(), newAccount.Address);

        // You can save this keystore information to securely store the account
        // For example, write it to a file or database
        Debug.Log($"Keystore: {keystore}");

        return newAccount;
    }
}
