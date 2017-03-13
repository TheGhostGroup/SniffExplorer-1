﻿using System;
using SniffExplorer.Enums;
using SniffExplorer.Packets.Parsing;

namespace SniffExplorer.Packets
{
    [ClientPacket(OpcodeClient.CMSG_AUTH_SESSION)]
    public struct UserClientAuthSession : IPacketStruct
    {
        public ulong DosResponse { get; set; }
        public ushort Build { get; set; }
        public byte BuildType { get; set; }
        public uint RegionID { get; set; }
        public uint BattlegroupID { get; set; }
        public uint RealmID { get; set; }
        [FixedSize(16)]
        public byte[] LocalChallenge { get; set; }
        [FixedSize(24)]
        public byte[] Digest { get; set; }
        [BitField(1)]
        public bool UsesIPv6 { get; set; }

        public int RealmJoinTicketSize { get; set; }
        [StreamedSize("RealmJoinTicketSize")]
        public byte[] RealmJoinTicket { get; set; }

        [Ignore]
        public DateTime Date { get; set; }

        [Ignore]
        public uint ConnectionID { get; set; }
    }
}