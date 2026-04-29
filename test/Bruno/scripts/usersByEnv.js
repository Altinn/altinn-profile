// Central place to define all test users and organizations per environment.
// This file can be required from Bruno scripts, e.g. in portal user.bru.

const usersByEnv = {
  at22: {
    user1: {
      AuthN_UserId: "20885478",
      AuthN_PartyId: "51118947",
      AuthN_Pid: "17902349936",
      AuthN_PartyUuid: "b8a3981b-9948-4109-88a9-679d90c4ab37",
      Party_OrgNo: "313605590",
      Party_PartyId: "51643854",
      Party_PartyUuid: "8027a287-e3f1-42ad-bb50-57b4c4584f13"
    }
    // Add more users for AT22 as needed (user2, org1, etc.)
  },

  at23: {
    user1: {
      AuthN_UserId: "20462603",
      AuthN_PartyId: "50891883",
      AuthN_Pid: "17902349936",
      AuthN_PartyUuid: "629fa2c0-27cd-40a2-ac6a-99bdf374dba2",
      Party_OrgNo: "313605590",
      Party_PartyId: "51519644",
      Party_PartyUuid: "4a1cfef9-82e1-4be9-96b9-b99f116f8350"
    }
    // Add more users for AT23 as needed.
  },

  at24: {
    user1: {
      AuthN_UserId: "20245418",
      AuthN_PartyId: "51074789",
      AuthN_Pid: "17902349936",
      AuthN_PartyUuid: "3155a6c7-0967-4c31-9cb3-0afe525d5899",
      Party_OrgNo: "313605590",
      Party_PartyId: "51605705",
      Party_PartyUuid: "e0347436-a499-49aa-b651-8c67c3c8d17e"
    }
    // Add more users for AT24 as needed.
  },

  tt02: {
    user1: {
      AuthN_UserId: "1597896",
      AuthN_PartyId: "51368167",
      AuthN_Pid: "17902349936",
      AuthN_PartyUuid: "200718e3-ab43-4d34-86ee-4cd8a975a55c",
      Party_OrgNo: "313605590",
      Party_PartyId: "51857221",
      Party_PartyUuid: "bb439b66-b467-4d9a-985f-e86738f93055"
    }
    // Add more TT02 users as needed.
  },

  dev: {
    // Local env (Local.bru)
    user1: {
      AuthN_UserId: "20002579",
      AuthN_PartyId: "",
      AuthN_Pid: "07837399275",
      AuthN_PartyUuid: "",
      Party_OrgNo: "313441571",
      Party_PartyId: "",
      Party_PartyUuid: ""
    }
    // Add more local users as needed.
  }
};

module.exports = {
  usersByEnv
};

