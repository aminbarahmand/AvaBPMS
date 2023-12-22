using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AvaBPMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class createDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PoolList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", maxLength: 50, nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoolList", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LaneList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PoolId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    RelatedRoleId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    RelatedUserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", maxLength: 50, nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaneList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LaneList_PoolList_PoolId",
                        column: x => x.PoolId,
                        principalTable: "PoolList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkFlowInstanseList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssignDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PoolId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkFlowInstanseList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkFlowInstanseList_PoolList_PoolId",
                        column: x => x.PoolId,
                        principalTable: "PoolList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkFlowNodeList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LaneId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    WorkFlowNodeType = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", maxLength: 50, nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkFlowNodeList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkFlowNodeList_LaneList_LaneId",
                        column: x => x.LaneId,
                        principalTable: "LaneList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransitionList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SourceWorkFlowNodeId = table.Column<int>(type: "int", nullable: false),
                    NextWorkFlowNodeId = table.Column<int>(type: "int", nullable: false),
                    Command = table.Column<int>(type: "int", nullable: false),
                    TransitionCondition = table.Column<int>(type: "int", nullable: false),
                    NodeValue = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransitionList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransitionList_WorkFlowNodeList_NextWorkFlowNodeId",
                        column: x => x.NextWorkFlowNodeId,
                        principalTable: "WorkFlowNodeList",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TransitionList_WorkFlowNodeList_SourceWorkFlowNodeId",
                        column: x => x.SourceWorkFlowNodeId,
                        principalTable: "WorkFlowNodeList",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorkFlowStepList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkFlowInstanseId = table.Column<int>(type: "int", nullable: false),
                    ReceiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WorkFlowNodeId = table.Column<int>(type: "int", nullable: false),
                    TransitionId = table.Column<int>(type: "int", nullable: false),
                    StepValue = table.Column<double>(type: "float", nullable: true),
                    IsALive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkFlowStepList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkFlowStepList_WorkFlowInstanseList_WorkFlowInstanseId",
                        column: x => x.WorkFlowInstanseId,
                        principalTable: "WorkFlowInstanseList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkFlowStepList_WorkFlowNodeList_WorkFlowNodeId",
                        column: x => x.WorkFlowNodeId,
                        principalTable: "WorkFlowNodeList",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LaneList_PoolId",
                table: "LaneList",
                column: "PoolId");

            migrationBuilder.CreateIndex(
                name: "IX_TransitionList_NextWorkFlowNodeId",
                table: "TransitionList",
                column: "NextWorkFlowNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_TransitionList_SourceWorkFlowNodeId",
                table: "TransitionList",
                column: "SourceWorkFlowNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkFlowInstanseList_PoolId",
                table: "WorkFlowInstanseList",
                column: "PoolId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkFlowNodeList_LaneId",
                table: "WorkFlowNodeList",
                column: "LaneId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkFlowStepList_WorkFlowInstanseId",
                table: "WorkFlowStepList",
                column: "WorkFlowInstanseId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkFlowStepList_WorkFlowNodeId",
                table: "WorkFlowStepList",
                column: "WorkFlowNodeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransitionList");

            migrationBuilder.DropTable(
                name: "WorkFlowStepList");

            migrationBuilder.DropTable(
                name: "WorkFlowInstanseList");

            migrationBuilder.DropTable(
                name: "WorkFlowNodeList");

            migrationBuilder.DropTable(
                name: "LaneList");

            migrationBuilder.DropTable(
                name: "PoolList");
        }
    }
}
