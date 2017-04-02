using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace qa_habrograb.Test
{
    [TestClass]
    public class GrabberCoreTest
    {
        ///*****  LangIdent  *****////
        [TestMethod]
        public void LangIdent_RusText_ru()
        {
            // Подготовка
            string russian_text = "Время подобно искусному управителю, непрестанно производящему новые таланты взамен исчезнувших.";
            string expected_result = "ru";
            GrabRequest gr = new GrabRequest();
            GrabConfig g_config = new GrabConfig();
            
            // Действие
            GrabberCore gc = new GrabberCore(gr, g_config);
            string actual_result = gc.LanguageIdentifier(russian_text);

            // Утверждение
            Assert.AreEqual(expected_result, actual_result);
        }

        [TestMethod]
        public void LangIdentNegative_RusText_en()
        {
            // Arrange
            string russian_text = "Время подобно искусному управителю, непрестанно производящему новые таланты взамен исчезнувших.";
            string not_expected_result = "en";
            GrabRequest gr = new GrabRequest();
            GrabConfig g_config = new GrabConfig();

            // Act
            GrabberCore gc = new GrabberCore(gr, g_config);
            string actual_result = gc.LanguageIdentifier(russian_text);

            // Assert
            Assert.AreNotEqual(not_expected_result, actual_result);
        }

        [TestMethod]
        public void LangIdent_EngText_en()
        {
            // Arrange
            string english_text = "The great Breakthrough in your life comes when you realize it that you can learn anything you need to" +
                                  "learn to accomplish any goal that you set for yourself. This means there are no limits on what you can" +
                                  "be, have or do.";
            string expected_result = "en";
            GrabRequest gr = new GrabRequest();
            GrabConfig g_config = new GrabConfig();

            // Act
            GrabberCore gc = new GrabberCore(gr, g_config);
            string actual_result = gc.LanguageIdentifier(english_text);

            // Assert
            Assert.AreEqual(expected_result, actual_result);
        }

        [TestMethod]
        public void LangIdentNegative_EngText_ru()
        {
            // Arrange
            string english_text = "The great Breakthrough in your life comes when you realize it that you can learn anything you need to" +
                                  "learn to accomplish any goal that you set for yourself. This means there are no limits on what you can" +
                                  "be, have or do.";
            string not_expected_result = "ru";
            GrabRequest gr = new GrabRequest();
            GrabConfig g_config = new GrabConfig();

            // Act
            GrabberCore gc = new GrabberCore(gr, g_config);
            string actual_result = gc.LanguageIdentifier(english_text);

            // Assert
            Assert.AreNotEqual(not_expected_result, actual_result);
        }

        ///*****  StringToDigit  *****////
        [TestMethod]
        public void StringToDigitPositive_string0_0()
        {
            // Arrange
            string input_string = "0";
            int expected_result = 0;

            // Act
            int actual_result = GrabberCore.StringToDigit(input_string);

            // Assert
            Assert.AreEqual(actual_result, expected_result);
        }

        [TestMethod]
        public void StringToDigitPositive_string21_21()
        {
            // Arrange
            string input_string = "21";
            int expected_result = 21;

            // Act
            int actual_result = GrabberCore.StringToDigit(input_string);

            // Assert
            Assert.AreEqual(actual_result, expected_result);
        }

        [TestMethod]
        public void StringToDigitPositive_stringPlus73_73()
        {
            // Arrange
            string input_string = "+73";
            int expected_result = 73;

            // Act
            int actual_result = GrabberCore.StringToDigit(input_string);

            // Assert
            Assert.AreEqual(actual_result, expected_result);
        }

        [TestMethod]
        public void StringToDigitPositive_stringPlus99999_99999()
        {
            // Arrange
            string input_string = "+99999";
            int expected_result = 99999;

            // Act
            int actual_result = GrabberCore.StringToDigit(input_string);

            // Assert
            Assert.AreEqual(actual_result, expected_result);
        }

        [TestMethod]
        public void StringToDigitPositive_stringMinus35_minus35()
        {
            // Arrange
            string input_string = "-35";
            int expected_result = -35;

            // Act
            int actual_result = GrabberCore.StringToDigit(input_string);

            // Assert
            Assert.AreEqual(actual_result, expected_result);
        }

        [TestMethod]
        public void StringToDigitPositive_string17and6k_17600()
        {
            // Arrange
            string input_string = "17,6k";
            int expected_result = 17600;

            // Act
            int actual_result = GrabberCore.StringToDigit(input_string);

            // Assert
            Assert.AreEqual(actual_result, expected_result);
        }

        [TestMethod]
        public void StringToDigitPositive_string9and3m_9300000()
        {
            // Arrange
            string input_string = "9,3m";
            int expected_result = 9300000;

            // Act
            int actual_result = GrabberCore.StringToDigit(input_string);

            // Assert
            Assert.AreEqual(actual_result, expected_result);
        }
    }
}
