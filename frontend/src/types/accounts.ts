export interface UserDto {
    id: string;
    userName: string;
    roles: string[];
    lockoutEnd: string | null;
}

export interface CreateUserPayload {
    username: string;
    password: string;
    roles: string[];
}
