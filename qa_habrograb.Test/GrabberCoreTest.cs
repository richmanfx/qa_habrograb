using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace qa_habrograb.Test
{
    [TestClass]
    public class GrabberCoreTest
    {
        [TestMethod]
        public void LangIdent_RusText_ru()
        {
            // Подготовка
            string russian_text = "Время подобно искусному управителю, непрестанно" +
                                  "производящему новые таланты взамен исчезнувших.";

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
        public void LangIdent_EngText_en()
        {
            // Arrange
            string english_text = "The great Breakthrough in your life comes when you" +
                                  "realize it that you can learn anything you need to" +
                                  "learn to accomplish any goal that you set for yourself." +
                                  "This means there are no limits on what you can be, have or do.";

            string expected_result = "en";

            GrabRequest gr = new GrabRequest();
            GrabConfig g_config = new GrabConfig();

            // Act
            GrabberCore gc = new GrabberCore(gr, g_config);
            string actual_result = gc.LanguageIdentifier(english_text);

            // Assert
            Assert.AreEqual(expected_result, actual_result);
        }

    }
}
