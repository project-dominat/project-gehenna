ent-StationAnchorBase = станционный якорь
    .desc = Удерживает станцию надёжно закреплённой в пространстве.

ent-StationAnchorIndestructible = { ent-StationAnchorBase }
    .suffix = Неразрушимый, Без питания
    .desc = { ent-StationAnchorBase.desc }

ent-StationAnchor = { ent-StationAnchorBase }
    .desc = { ent-StationAnchorBase.desc }

ent-StationAnchorOff = { ent-StationAnchor }
    .suffix = Выключен
    .desc = { ent-StationAnchor.desc }
