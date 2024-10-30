using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLinkr.TwitchAPI.APIResponseModels
{
	internal class UserResponseModel
	{
		public UserDataModel[] Data = default!;
	}

	internal class UserDataModel
	{
        public string Id { get; set; } = default!;
        public string Login { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public string Type { get; set; } = default!;
        public string BroadcasterType { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string ProfileImageUrl { get; set; } = default!;
        public string OfflineImageUrl { get; set; } = default!;
        public string Email {  get; set; } = default!;
        public string CreatedAt { get; set; } = default!;
        public DateTime CreatedDate => DateTime.Parse(CreatedAt);
    }


}
