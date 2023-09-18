using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Nethereum.Web3.Accounts;
using System.Numerics;
using Nethereum.Util;
using System.Threading.Tasks;
using System; 
using Nethereum.RPC.Eth.Transactions;
using UnityEngine;


public class WalletFactory : MonoBehaviour
{
    private Web3 web3;
    private Contract _contract;
    private string contractAddress = "0x08C93c5d92Ba5Ff7470E4060A930921148dE8BBd";  // Replace this with your actual contract address
    private string abi = @"[{""inputs"": [{""internalType"": ""address"", ""name"": ""_owner"", ""type"": ""address""}]," +
    @"""stateMutability"": ""nonpayable"", ""type"": ""constructor""}, {""anonymous"": false, ""inputs"":" +
    @"[{""indexed"": true, ""internalType"": ""address"", ""name"": ""wallet"", ""type"": ""address""}," +
    @"{""indexed"": true, ""internalType"": ""address"", ""name"": ""owner"", ""type"": ""address""}," +
    @"{""indexed"": false, ""internalType"": ""uint256"", ""name"": ""index"", ""type"": ""uint256""}]," +
    @"""name"": ""AccountCreation"", ""type"": ""event""}, {""anonymous"": false, ""inputs"":" +
    @"[{""indexed"": false, ""internalType"": ""address"", ""name"": ""newImplementation"", ""type"":" +
    @"""address""}], ""name"": ""ImplementationSet"", ""type"": ""event""}, {""anonymous"": false," +
    @"""inputs"": [{""indexed"": false, ""internalType"": ""address"", ""name"": ""newOwner"", ""type"":" +
    @"""address""}], ""name"": ""OwnerChanged"", ""type"": ""event""}, {""inputs"": [], ""name"":" +
    @"""accountCreationCode"", ""outputs"": [{""internalType"": ""bytes"", ""name"": """", ""type"":" +
    @"""bytes""}], ""stateMutability"": ""pure"", ""type"": ""function""}, {""inputs"": [], ""name"":" +
    @"""accountImplementation"", ""outputs"": [{""internalType"": ""address"", ""name"": """", ""type"":" +
    @"""address""}], ""stateMutability"": ""view"", ""type"": ""function""}, {""inputs"":" +
    @"[{""internalType"": ""address"", ""name"": ""_newOwner"", ""type"": ""address""}], ""name"":" +
    @"""changeOwner"", ""outputs"": [], ""stateMutability"": ""nonpayable"", ""type"": ""function""}," +
    @"{""inputs"": [{""internalType"": ""address"", ""name"": ""_impl"", ""type"": ""address""}]," +
    @"""name"": ""checkImplementation"", ""outputs"": [{""internalType"": ""bool"", ""name"": """"," +
    @"""type"": ""bool""}], ""stateMutability"": ""view"", ""type"": ""function""}, {""inputs"":" +
    @"[{""internalType"": ""address"", ""name"": ""_owner"", ""type"": ""address""}, {""internalType"":" +
    @"""uint256"", ""name"": ""_index"", ""type"": ""uint256""}], ""name"": ""createAccount""," +
    @"""outputs"": [{""internalType"": ""address"", ""name"": ""ret"", ""type"": ""address""}]," +
    @"""stateMutability"": ""nonpayable"", ""type"": ""function""}, {""inputs"": [{""internalType"":" +
    @"""address"", ""name"": ""_owner"", ""type"": ""address""}, {""internalType"": ""uint256"", ""name"":" +
    @"""_index"", ""type"": ""uint256""}], ""name"": ""getAddress"", ""outputs"": [{""internalType"":" +
    @"""address"", ""name"": ""proxy"", ""type"": ""address""}], ""stateMutability"": ""view"", ""type"":" +
    @"""function""}, {""inputs"": [], ""name"": ""owner"", ""outputs"": [{""internalType"": ""address""," +
    @"""name"": """", ""type"": ""address""}], ""stateMutability"": ""view"", ""type"": ""function""}," +
    @"{""inputs"": [{""internalType"": ""contract EtherspotWallet"", ""name"": ""_newImpl"", ""type"":" +
    @"""address""}], ""name"": ""setImplementation"", ""outputs"": [], ""stateMutability"":" +
    @"""nonpayable"", ""type"": ""function""}]";  // Replace this with your actual ABI
void Start()
{
    // Initialize and execute asynchronous functions
    Initialize("https://goerli-bundler.etherspot.io", "e9aadb1a5d5e19e271949b28c4e5bf8139d52de75e3f3c547d977cb08ce881c3");  // Replace these placeholders
    ExecuteAsyncFunctions();
}

async void ExecuteAsyncFunctions()
{
    try
        {
            await AccountCreationCode();
            string ownerAddress = "0xdFf857C5a0c56B4235A4847D49b3EabD8788f333";
            uint index = 1;
            await CreateAccount(ownerAddress, index);
            await GetAddress(ownerAddress, index);
        }
        catch (Exception ex)
        {
            Debug.LogError($"An error occurred: {ex.Message}");
        }
}

    public void Initialize(string rpcUrl, string privateKey)
    {
        var account = new Account(privateKey);
        web3 = new Web3(account, rpcUrl);
        _contract = web3.Eth.GetContract(abi, contractAddress);
    }

    public async Task AccountCreationCode()
    {
        var function = _contract.GetFunction("accountCreationCode");
        var result = await function.CallAsync<byte[]>();
        string hexString = BitConverter.ToString(result).Replace("-", "");
        Debug.Log($"Account Creation Code: {hexString}");
    }
public async Task CreateAccount(string ownerAddress, uint index)
{
    var function = _contract.GetFunction("createAccount");
    var gasEstimate = await function.EstimateGasAsync(new object[] { ownerAddress, index });

    // Fetch the current nonce
    var nonce = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(ownerAddress);

    // Create the raw transaction input
    var txInput = new TransactionInput
    {
        To = contractAddress,
        From = ownerAddress,
        Gas = new HexBigInteger(gasEstimate.Value),
        Data = function.GetData(new object[] { ownerAddress, index }),
        Nonce = new HexBigInteger(nonce)
    };

    try
    {
        // Sign and send the transaction
        var transactionHash = await web3.Eth.Transactions.SendTransaction.SendRequestAsync(txInput);

        // Create a payload or object containing the transaction hash
        var userOp = new
        {
            Operation = txInput.Data,
            TransactionHash = transactionHash
        };

        // Print the userOp for debugging
        Debug.Log($"User operation details: {Newtonsoft.Json.JsonConvert.SerializeObject(userOp)}");
    }
    catch (Exception ex)
    {
        Debug.LogError($"An error occurred: {ex.Message}");
    }
}






    public async Task GetAddress(string ownerAddress, uint index)
{
    var function = _contract.GetFunction("getAddress");
   var result = await function.CallAsync<string>(new object[] { ownerAddress, index });

    Debug.Log($"Address: {result}");
}
  public async Task SetImplementation(string implementationAddress)
    {
        var function = _contract.GetFunction("setImplementation");
       var result = await function.CallAsync<string>(new object[] { implementationAddress});
        Debug.Log($"SetImplementation: {result}");
    }

    public async Task<bool> CheckImplementation(string addressToCheck)
    {
        var function = _contract.GetFunction("checkImplementation");
        var result = await function.CallAsync<bool>(new object[] { addressToCheck });
        
        Debug.Log($"Is implementation set: {result}");
        return result;
    }

}

