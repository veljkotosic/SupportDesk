import type {UserType} from "@/types/user/userType.ts";

export interface User {
  userId: string;
  userName: string;
  email: string;
  type: UserType;
}
