
using System;
using SigneWordBotAspCore.Utils;
using Xunit;
using Xunit.Abstractions;

namespace SingleWordUnitTest
{
    public class CryptoTest
    {
        private readonly ITestOutputHelper _output;

        public CryptoTest(ITestOutputHelper output)
        {
            _output = output;
        }
        
        public void EncryptTest()
        {
            var strForCrypting = "qwdasvotewf!!2123asdvFDFf'жжжуйxX2";
            var password = "qwerrtt!!!!п!xz";
            
            
            var resultOfEncrypt = Crypting.Encrypt(strForCrypting, password);
            var resultOfDecrypt = Crypting.Decrypt(resultOfEncrypt, password);

            
            Assert.Equal(strForCrypting, resultOfDecrypt);


            strForCrypting = "ч";
            password = "t";
            resultOfEncrypt = Crypting.Encrypt(strForCrypting, password); 
            resultOfDecrypt = Crypting.Decrypt(resultOfEncrypt, password);

            _output.WriteLine($"ecr: {resultOfEncrypt}");

            Assert.Equal(strForCrypting, resultOfDecrypt);
            
            strForCrypting = "this IS LONG_STRONG!!!!dqw124r___password_пароль_--паоль,,,,!?55=-?ч?fмсчмынпыпглгуисмкйуглрдатйй1431654у1йуццуццйеа?342'";
            password = "LONG_STRONGLONG_STRONGLONG_STRONGLONG_STRONG";
            resultOfEncrypt = Crypting.Encrypt(strForCrypting, password); 
            resultOfDecrypt = Crypting.Decrypt(resultOfEncrypt, password);
            _output.WriteLine($"\n\necr: {resultOfEncrypt}");

            Assert.Equal(strForCrypting, resultOfDecrypt);
            
            strForCrypting = "passQwe!12e";
            password = "12345";
            resultOfEncrypt = Crypting.Encrypt(strForCrypting, password); 
            resultOfDecrypt = Crypting.Decrypt(resultOfEncrypt, "23456");

            
            Assert.NotEqual(strForCrypting, resultOfDecrypt);
            
            _output.WriteLine($"WRONG pass:\n{resultOfDecrypt}");
        }

        
    }
}