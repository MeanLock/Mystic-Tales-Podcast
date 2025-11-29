// @ts-nocheck

import type { AccountFromAPI, AccountUI } from "../types/account";

export const loginInfoMap = [
  { email: "user1@gmail.com", password: "@A123456", id: 1 },
  { email: "user2@gmail.com", password: "@A123456", id: 2 },
  { email: "user3@gmail.com", password: "@A123456", id: 3 },
  { email: "user4@gmail.com", password: "@A123456", id: 4 },
];

export const basicUser: AccountUI = {
  Id: 1,
  Email: "soryzzmikely@gmail.com",
  Role: {
    Id: 1,
    Name: "Customer",
  },
  FullName: "Hoàng Minh Lộc",
  Dob: "2004-03-10", //Format "YYYY-MM-DD"
  Gender: "male",
  Address: "S5.01B - Vinhomes Grandpark, Nguyễn Xiển, Quận 9",
  Phone: "0896893636",
  Balance: 100000,
  ImageUrl:
    "https://i.pinimg.com/1200x/c1/28/60/c1286068913d4a583cae08c7ae77ac18.jpg",
  IsVerified: true,
  GoogleId: null,
  PodcastListenSlot: 10,
  ViolationPoint: 0,
  ViolationLevel: 0,
  LastViolationPointChanged: null, // Format ISO String
  LastViolationLevelChanged: null, // Format ISO String
  LastPodcastListenSlotChanged: null,
  DeactivatedAt: null, // Format ISO String
  CreatedAt: "", // Format ISO String
  UpdatedAt: "", // Format ISO String
  IsPodcaster: false, //Để check xem User có phải podcaster không, hay là User thường
};

export const podcasterUser: AccountUI = {
  Id: 2,
  Email: "tranquangphatthinh@gmail.com",
  Role: {
    Id: 1,
    Name: "Customer",
  },
  FullName: "Trần Quang Phát Thịnh",
  Dob: "2004-02-23", //Format "YYYY-MM-DD"
  Gender: "male",
  Address: "Easy Home, Leng Keng, Tiktok",
  Phone: "0816456466",
  Balance: 100000,
  ImageUrl:
    "https://i.pinimg.com/1200x/2b/e4/6e/2be46efa6be1bb6a5a06d61c78d18801.jpg",
  IsVerified: true,
  GoogleId: null,
  PodcastListenSlot: 10,
  ViolationPoint: 0,
  ViolationLevel: 0,
  LastViolationPointChanged: null, // Format ISO String
  LastViolationLevelChanged: null, // Format ISO String
  LastPodcastListenSlotChanged: null,
  DeactivatedAt: null, // Format ISO String
  CreatedAt: "", // Format ISO String
  UpdatedAt: "", // Format ISO String
  IsPodcaster: true, //Để check xem User có phải podcaster không, hay là User thường
};

export const unVerifiedUser: AccountUI = {
  Id: 3,
  Email: "hanguyenhao@gmail.com",
  Role: {
    Id: 1,
    Name: "Customer",
  },
  FullName: "Hà Nguyên Hào",
  Dob: "2004-04-20", //Format "YYYY-MM-DD"
  Gender: "male",
  Address: "S3.05, Vinhomes Grandpark, Nguyễn Xiển, Quận 9",
  Phone: "0917759357",
  Balance: 0,
  ImageUrl:
    "https://i.pinimg.com/736x/da/66/46/da66461e194004bb90fc8103b87bf70d.jpg",
  IsVerified: false,
  GoogleId: null,
  PodcastListenSlot: 10,
  ViolationPoint: 0,
  ViolationLevel: 0,
  LastViolationPointChanged: null, // Format ISO String
  LastViolationLevelChanged: null, // Format ISO String
  LastPodcastListenSlotChanged: null,
  DeactivatedAt: null, // Format ISO String
  CreatedAt: "", // Format ISO String
  UpdatedAt: "", // Format ISO String
  IsPodcaster: false, //Để check xem User có phải podcaster không, hay là User thường
};

export const outOfListenSlotUser: AccountUI = {
  Id: 4,
  Email: "duongxuanback@gmail.com",
  Role: {
    Id: 1,
    Name: "Customer",
  },
  FullName: "Dương Xuân Bách",
  Dob: "2004-04-20", //Format "YYYY-MM-DD"
  Gender: "male",
  Address: "S3.05, Vinhomes Grandpark, Nguyễn Xiển, Quận 9",
  Phone: "0917759357",
  Balance: 0,
  ImageUrl:
    "https://i.pinimg.com/736x/22/39/bc/2239bc8c3de74de0c9f376db7799685f.jpg",
  IsVerified: true,
  GoogleId: null,
  PodcastListenSlot: 0,
  ViolationPoint: 0,
  ViolationLevel: 0,
  LastViolationPointChanged: null, // Format ISO String
  LastViolationLevelChanged: null, // Format ISO String
  LastPodcastListenSlotChanged: null,
  DeactivatedAt: "ădadawdawdw", // Format ISO String
  CreatedAt: "", // Format ISO String
  UpdatedAt: "", // Format ISO String
  IsPodcaster: false, //Để check xem User có phải podcaster không, hay là User thường
};

export const mockUsers: AccountUI[] = [
  basicUser,
  outOfListenSlotUser,
  podcasterUser,
  unVerifiedUser,
];
