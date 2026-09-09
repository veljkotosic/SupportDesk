import type {UserRole} from "@/types/user/userRole.ts";

export interface User {
  userId: string;
  userName: string;
  email: string;
  role: UserRole;
}
