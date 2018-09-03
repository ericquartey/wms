using System;
using AutoMapper;
using Ferretto.Common.Utils.Testing;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.DAL.Interfaces;
using Ferretto.Common.Modules.BLL;
using Ferretto.Common.Modules.BLL.Services;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Feretto.Common.Modules.BLL.Tests
{
  [TestClass]
  [TestCategory("Business")]
  public class ItemServiceTest : UnityTest
  {
    private IItemsService itemsService;
    private static readonly int itemId = 1;
    private static readonly string itemDescription = "Description";
    private static readonly int itemHeight = 1;

    private static readonly int itemIdInvalid = 2;
    private static readonly int itemHeightInvalid = -1;

    private static readonly Ferretto.Common.Modules.BLL.Models.Item bllObject = new Ferretto.Common.Modules.BLL.Models.Item
    {
      Id = itemId,
      Description = itemDescription,
      Height = itemHeight
    };

    private static readonly Ferretto.Common.DAL.Models.Item dalObject = new Ferretto.Common.DAL.Models.Item
    {
      Id = itemId,
      Description = itemDescription,
      Height = itemHeight
    };

    [TestInitialize]
    public override void Initialize()
    {
      base.Initialize();

      var mockRepository = new Mock<IItemsRepository>();
      mockRepository.SetReturnsDefault(dalObject);

      mockRepository.Setup(a => a.GetById(itemId)).Returns(dalObject);

      mockRepository.Setup(a => a.GetById(itemIdInvalid)).Returns(
        new Ferretto.Common.DAL.Models.Item
        {
          Id = itemIdInvalid,
          Height = itemHeightInvalid
        }
      );

      this.Container.RegisterInstance(mockRepository.Object);
      this.Container.RegisterType<IItemsService, ItemsService>();

      this.itemsService = ServiceLocator.Current.GetInstance<IItemsService>();

      Mapper.Initialize(config => config.AddProfile<BusinessLogicAutoMapperProfile>());
    }

    [TestCleanup]
    public void Cleanup()
    {
      Mapper.Reset();
    }

    [TestMethod]
    public void TestGetById()
    {
      var item = this.itemsService.GetById(itemId);

      Assert.AreEqual(itemId, item.Id);
      Assert.AreEqual(itemDescription, item.Description);
      Assert.AreEqual(itemHeight, item.Height);
    }

    [TestMethod]
    public void TestCreate()
    {
      var createdBllObject = this.itemsService.Create(bllObject);

      Assert.AreEqual(bllObject.Id, createdBllObject.Id);
      Assert.AreEqual(bllObject.Description, createdBllObject.Description);
      Assert.AreEqual(bllObject.Height, createdBllObject.Height);
    }

    [TestMethod]
    public void TestGetByIdWithInvalidRepositoryData()
    {
      Assert.ThrowsException<AutoMapperMappingException>(
        () => this.itemsService.GetById(itemIdInvalid),
        $"Business object should throw exception because repository is returning an object with invalid height of {itemHeightInvalid}.");
    }
  }
}
