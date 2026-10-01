export const EFikaLocation = {
    None: 0,
    Hideout: 1,
    Customs: 2,
    Factory: 3,
    Interchange: 4,
    Labyrinth: 5,
    Lighthouse: 6,
    Reserve: 7,
    Streets: 8,
    Woods: 9,
    GroundZero: 10,
    Shoreline: 11,
    Laboratory: 12,
} as const;

export type EFikaLocation = (typeof EFikaLocation)[keyof typeof EFikaLocation];

export interface OnlinePlayer {
    profileId: string;
    nickname: string;
    level: number;
    location: EFikaLocation;
}
