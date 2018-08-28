using System;
using System.Collections.Generic;
using System.Windows.Media;
using Ferretto.Common.Utils.Testing;
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
  [TestCategory("Business")]
  public class ImageServiceTest : UnityTest
  {
    private IImageService imageService;
    static readonly string imageFilePath = "image.png";

    [TestInitialize]
    public override void Initialize()
    {
      base.Initialize();

      var mockImageRepository = new Mock<IImageRepository>();
      mockImageRepository
        .Setup(a => a.GetById(imageFilePath)).Throws<InvalidOperationException>();

      this.Container.RegisterInstance(mockImageRepository.Object);
      this.Container.RegisterType<IImageService, ImageService>();

      this.imageService = ServiceLocator.Current.GetInstance<IImageService>();
    }

    [TestMethod]
    public void TestGetImage()
    {
      Assert.ThrowsException<InvalidOperationException>(() => this.imageService.GetImage(imageFilePath));
    }
  }
}
