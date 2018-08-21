using System;
using System.Collections.Generic;
using System.Windows.Media;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.DAL.Interfaces;
using Ferretto.Common.Modules.BLL.Services;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Feretto.Common.Modules.BLL.Tests
{
  [TestClass]
  [TestCategory("Integration Test")]
  public class ImageServiceTest : UnityTest
  {
    private IImageService imageService;
    static readonly string imageFilePath = "image.png";

    [TestInitialize]
    public void Initialize()
    {
      var mockImageRepository = new Mock<IImageRepository>();
      mockImageRepository
        .Setup(a => a.GetById(imageFilePath)).Throws<InvalidOperationException>();

      container.RegisterInstance(mockImageRepository.Object);
      container.RegisterType<IImageService, ImageService>();

      this.imageService = ServiceLocator.Current.GetInstance<IImageService>();
    }

    [TestMethod]
    public void TestGetImage()
    {
      Assert.ThrowsException<InvalidOperationException>(() => this.imageService.GetImage(imageFilePath));
    }
  }
}
