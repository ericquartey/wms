using AutoMapper;

namespace Ferretto.Common.Modules.BLL
{
    public class BusinessLogicAutoMapperProfile : Profile
    {
        #region Constructors

        public BusinessLogicAutoMapperProfile()
        {
            this.CreateMap<DataModels.AbcClass, Models.AbcClass>();
            this.CreateMap<Models.AbcClass, DataModels.AbcClass>();

            this.CreateMap<DataModels.Compartment, Models.Compartment>();
            this.CreateMap<Models.Compartment, DataModels.Compartment>();

            this.CreateMap<DataAccess.ItemDTO, Models.Item>();
            this.CreateMap<Models.Item, DataAccess.ItemDTO>();

            this.CreateMap<DataModels.Item, Models.Item>();
            this.CreateMap<Models.Item, DataModels.Item>();

            this.CreateMap<DataModels.Item, Models.ItemDetails>();
            this.CreateMap<Models.ItemDetails, DataModels.Item>();

            this.CreateMap<DataModels.ItemManagementType, Models.ItemManagementType>();
            this.CreateMap<Models.ItemManagementType, DataModels.ItemManagementType>();

            this.CreateMap<DataModels.MeasureUnit, Models.MeasureUnit>();
            this.CreateMap<Models.MeasureUnit, DataModels.MeasureUnit>();

            this.CreateMissingTypeMaps = true;
        }

        #endregion Constructors
    }
}
