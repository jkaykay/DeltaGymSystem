using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymSystem.Api.Migrations
{
    /// <inheritdoc />
    public partial class DbSetAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Branch_BranchId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendance_AspNetUsers_UserId",
                table: "Attendance");

            migrationBuilder.DropForeignKey(
                name: "FK_Booking_AspNetUsers_UserId",
                table: "Booking");

            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Session_SessionId",
                table: "Booking");

            migrationBuilder.DropForeignKey(
                name: "FK_Class_AspNetUsers_UserId",
                table: "Class");

            migrationBuilder.DropForeignKey(
                name: "FK_Equipment_Room_RoomId",
                table: "Equipment");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_AspNetUsers_UserId",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Subscription_SubId",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Room_Branch_BranchId",
                table: "Room");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedule_AspNetUsers_UserId",
                table: "Schedule");

            migrationBuilder.DropForeignKey(
                name: "FK_Session_Class_ClassId",
                table: "Session");

            migrationBuilder.DropForeignKey(
                name: "FK_Session_Room_RoomId",
                table: "Session");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscription_AspNetUsers_UserId",
                table: "Subscription");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscription_Tier_TierName",
                table: "Subscription");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tier",
                table: "Tier");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subscription",
                table: "Subscription");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Session",
                table: "Session");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Schedule",
                table: "Schedule");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Room",
                table: "Room");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payment",
                table: "Payment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Equipment",
                table: "Equipment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Class",
                table: "Class");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Branch",
                table: "Branch");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Booking",
                table: "Booking");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Attendance",
                table: "Attendance");

            migrationBuilder.RenameTable(
                name: "Tier",
                newName: "Tiers");

            migrationBuilder.RenameTable(
                name: "Subscription",
                newName: "Subscriptions");

            migrationBuilder.RenameTable(
                name: "Session",
                newName: "Sessions");

            migrationBuilder.RenameTable(
                name: "Schedule",
                newName: "Schedules");

            migrationBuilder.RenameTable(
                name: "Room",
                newName: "Rooms");

            migrationBuilder.RenameTable(
                name: "Payment",
                newName: "Payments");

            migrationBuilder.RenameTable(
                name: "Equipment",
                newName: "Equipments");

            migrationBuilder.RenameTable(
                name: "Class",
                newName: "Classes");

            migrationBuilder.RenameTable(
                name: "Branch",
                newName: "Branches");

            migrationBuilder.RenameTable(
                name: "Booking",
                newName: "Bookings");

            migrationBuilder.RenameTable(
                name: "Attendance",
                newName: "Attendances");

            migrationBuilder.RenameIndex(
                name: "IX_Subscription_UserId",
                table: "Subscriptions",
                newName: "IX_Subscriptions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Subscription_TierName",
                table: "Subscriptions",
                newName: "IX_Subscriptions_TierName");

            migrationBuilder.RenameIndex(
                name: "IX_Session_RoomId",
                table: "Sessions",
                newName: "IX_Sessions_RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_Session_ClassId",
                table: "Sessions",
                newName: "IX_Sessions_ClassId");

            migrationBuilder.RenameIndex(
                name: "IX_Schedule_UserId",
                table: "Schedules",
                newName: "IX_Schedules_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Room_BranchId",
                table: "Rooms",
                newName: "IX_Rooms_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_UserId",
                table: "Payments",
                newName: "IX_Payments_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_SubId",
                table: "Payments",
                newName: "IX_Payments_SubId");

            migrationBuilder.RenameIndex(
                name: "IX_Equipment_RoomId",
                table: "Equipments",
                newName: "IX_Equipments_RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_Class_UserId",
                table: "Classes",
                newName: "IX_Classes_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Booking_UserId",
                table: "Bookings",
                newName: "IX_Bookings_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Booking_SessionId",
                table: "Bookings",
                newName: "IX_Bookings_SessionId");

            migrationBuilder.RenameIndex(
                name: "IX_Attendance_UserId",
                table: "Attendances",
                newName: "IX_Attendances_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tiers",
                table: "Tiers",
                column: "TierName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions",
                column: "SubId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sessions",
                table: "Sessions",
                column: "SessionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Schedules",
                table: "Schedules",
                column: "ScheduleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Rooms",
                table: "Rooms",
                column: "RoomId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payments",
                table: "Payments",
                column: "PaymentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Equipments",
                table: "Equipments",
                column: "EquipmentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Classes",
                table: "Classes",
                column: "ClassId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Branches",
                table: "Branches",
                column: "BranchId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bookings",
                table: "Bookings",
                column: "BookingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Attendances",
                table: "Attendances",
                column: "AttendanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Branches_BranchId",
                table: "AspNetUsers",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_AspNetUsers_UserId",
                table: "Attendances",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_AspNetUsers_UserId",
                table: "Bookings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Sessions_SessionId",
                table: "Bookings",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "SessionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_AspNetUsers_UserId",
                table: "Classes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_Rooms_RoomId",
                table: "Equipments",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "RoomId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_AspNetUsers_UserId",
                table: "Payments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Subscriptions_SubId",
                table: "Payments",
                column: "SubId",
                principalTable: "Subscriptions",
                principalColumn: "SubId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Branches_BranchId",
                table: "Rooms",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_AspNetUsers_UserId",
                table: "Schedules",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Classes_ClassId",
                table: "Sessions",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "ClassId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Rooms_RoomId",
                table: "Sessions",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "RoomId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_AspNetUsers_UserId",
                table: "Subscriptions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Tiers_TierName",
                table: "Subscriptions",
                column: "TierName",
                principalTable: "Tiers",
                principalColumn: "TierName",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Branches_BranchId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_AspNetUsers_UserId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_AspNetUsers_UserId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Sessions_SessionId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Classes_AspNetUsers_UserId",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_Rooms_RoomId",
                table: "Equipments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_AspNetUsers_UserId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Subscriptions_SubId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Branches_BranchId",
                table: "Rooms");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_AspNetUsers_UserId",
                table: "Schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Classes_ClassId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Rooms_RoomId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_AspNetUsers_UserId",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Tiers_TierName",
                table: "Subscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tiers",
                table: "Tiers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sessions",
                table: "Sessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Schedules",
                table: "Schedules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Rooms",
                table: "Rooms");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payments",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Equipments",
                table: "Equipments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Classes",
                table: "Classes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Branches",
                table: "Branches");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bookings",
                table: "Bookings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Attendances",
                table: "Attendances");

            migrationBuilder.RenameTable(
                name: "Tiers",
                newName: "Tier");

            migrationBuilder.RenameTable(
                name: "Subscriptions",
                newName: "Subscription");

            migrationBuilder.RenameTable(
                name: "Sessions",
                newName: "Session");

            migrationBuilder.RenameTable(
                name: "Schedules",
                newName: "Schedule");

            migrationBuilder.RenameTable(
                name: "Rooms",
                newName: "Room");

            migrationBuilder.RenameTable(
                name: "Payments",
                newName: "Payment");

            migrationBuilder.RenameTable(
                name: "Equipments",
                newName: "Equipment");

            migrationBuilder.RenameTable(
                name: "Classes",
                newName: "Class");

            migrationBuilder.RenameTable(
                name: "Branches",
                newName: "Branch");

            migrationBuilder.RenameTable(
                name: "Bookings",
                newName: "Booking");

            migrationBuilder.RenameTable(
                name: "Attendances",
                newName: "Attendance");

            migrationBuilder.RenameIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscription",
                newName: "IX_Subscription_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Subscriptions_TierName",
                table: "Subscription",
                newName: "IX_Subscription_TierName");

            migrationBuilder.RenameIndex(
                name: "IX_Sessions_RoomId",
                table: "Session",
                newName: "IX_Session_RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_Sessions_ClassId",
                table: "Session",
                newName: "IX_Session_ClassId");

            migrationBuilder.RenameIndex(
                name: "IX_Schedules_UserId",
                table: "Schedule",
                newName: "IX_Schedule_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Rooms_BranchId",
                table: "Room",
                newName: "IX_Room_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_UserId",
                table: "Payment",
                newName: "IX_Payment_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_SubId",
                table: "Payment",
                newName: "IX_Payment_SubId");

            migrationBuilder.RenameIndex(
                name: "IX_Equipments_RoomId",
                table: "Equipment",
                newName: "IX_Equipment_RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_Classes_UserId",
                table: "Class",
                newName: "IX_Class_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_UserId",
                table: "Booking",
                newName: "IX_Booking_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_SessionId",
                table: "Booking",
                newName: "IX_Booking_SessionId");

            migrationBuilder.RenameIndex(
                name: "IX_Attendances_UserId",
                table: "Attendance",
                newName: "IX_Attendance_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tier",
                table: "Tier",
                column: "TierName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subscription",
                table: "Subscription",
                column: "SubId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Session",
                table: "Session",
                column: "SessionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Schedule",
                table: "Schedule",
                column: "ScheduleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Room",
                table: "Room",
                column: "RoomId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payment",
                table: "Payment",
                column: "PaymentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Equipment",
                table: "Equipment",
                column: "EquipmentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Class",
                table: "Class",
                column: "ClassId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Branch",
                table: "Branch",
                column: "BranchId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Booking",
                table: "Booking",
                column: "BookingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Attendance",
                table: "Attendance",
                column: "AttendanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Branch_BranchId",
                table: "AspNetUsers",
                column: "BranchId",
                principalTable: "Branch",
                principalColumn: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_AspNetUsers_UserId",
                table: "Attendance",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_AspNetUsers_UserId",
                table: "Booking",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Session_SessionId",
                table: "Booking",
                column: "SessionId",
                principalTable: "Session",
                principalColumn: "SessionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Class_AspNetUsers_UserId",
                table: "Class",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Equipment_Room_RoomId",
                table: "Equipment",
                column: "RoomId",
                principalTable: "Room",
                principalColumn: "RoomId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_AspNetUsers_UserId",
                table: "Payment",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_Subscription_SubId",
                table: "Payment",
                column: "SubId",
                principalTable: "Subscription",
                principalColumn: "SubId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Room_Branch_BranchId",
                table: "Room",
                column: "BranchId",
                principalTable: "Branch",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedule_AspNetUsers_UserId",
                table: "Schedule",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Session_Class_ClassId",
                table: "Session",
                column: "ClassId",
                principalTable: "Class",
                principalColumn: "ClassId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Session_Room_RoomId",
                table: "Session",
                column: "RoomId",
                principalTable: "Room",
                principalColumn: "RoomId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscription_AspNetUsers_UserId",
                table: "Subscription",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscription_Tier_TierName",
                table: "Subscription",
                column: "TierName",
                principalTable: "Tier",
                principalColumn: "TierName",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
