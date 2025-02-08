using BackendExamHub.Data;
using BackendExamHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;

namespace BackendExamHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MyOfficeController : ControllerBase
    {
        private readonly BackendExamHubDbContext _context;

        public MyOfficeController(BackendExamHubDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 取得所有使用者資訊
        /// </summary>
        [HttpGet("GetUsers")]
        public async Task<ActionResult<IEnumerable<MyOfficeACPD>>> GetUsers()
        {
            return await _context.MyOfficeACPD.ToListAsync();
        }

        /// <summary>
        /// 產生新的 ACPD_SID (透過 Stored Procedure `NEWSID`)
        /// </summary>
        private async Task<string> GenerateNewSID()
        {
            string newSID = string.Empty;
            using (var connection = (SqlConnection)_context.Database.GetDbConnection())
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("NEWSID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@TableName", "MyOffice_ACPD"));
                    var outputParam = new SqlParameter("@ReturnSID", SqlDbType.NVarChar, 20) { Direction = ParameterDirection.Output };
                    command.Parameters.Add(outputParam);

                    await command.ExecuteNonQueryAsync();
                    newSID = outputParam.Value.ToString() ?? string.Empty;
                }
            }
            return newSID;
        }

        /// <summary>
        /// 新增使用者 (透過 Stored Procedure)
        /// </summary>
        [HttpPost("InsertUser")]
        public async Task<IActionResult> InsertUser([FromBody] JsonElement jsonData)
        {
            var jsonString = jsonData.GetRawText();
            string newSID = await GenerateNewSID(); // 產生新的 SID
            string logMessage = ""; 

            try
            {
                using (var connection = (SqlConnection)_context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("InsertMyOfficeACPD", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@JsonData", jsonString));
                        command.Parameters.Add(new SqlParameter("@ACPD_SID", newSID)); // 傳入新 SID

                        await command.ExecuteNonQueryAsync();
                    }
                }
                logMessage = $"Insert Success: {jsonString}";
                return Ok(new { message = "User inserted successfully", ACPD_SID = newSID });
            }
            catch (Exception ex)
            {
                logMessage = $"Insert Failed: {ex.Message}";
                return StatusCode(500, new { message = "Insert failed", error = ex.Message });
            }
            finally
            {
                await LogExecution("InsertMyOfficeACPD", logMessage);
            }
        }

        /// <summary>
        /// 更新使用者 (透過 Stored Procedure)
        /// </summary>
        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] JsonElement jsonData)
        {
            var jsonString = jsonData.GetRawText();
            string logMessage = ""; 

            try
            {
                using (var connection = (SqlConnection)_context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("UpdateMyOfficeACPD", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@JsonData", jsonString));

                        await command.ExecuteNonQueryAsync();
                    }
                }
                logMessage = $"Update Success: {jsonString}";
                return Ok(new { message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                logMessage = $"Update Failed: {ex.Message}";
                return StatusCode(500, new { message = "Update failed", error = ex.Message });
            }
            finally
            {
                await LogExecution("UpdateMyOfficeACPD", logMessage);
            }
        }

        /// <summary>
        /// 刪除使用者 (透過 Stored Procedure)
        /// </summary>
        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser([FromBody] JsonElement jsonData)
        {
            var jsonString = jsonData.GetRawText();
            string logMessage = ""; 

            try
            {
                using (var connection = (SqlConnection)_context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("DeleteMyOfficeACPD", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@JsonData", jsonString));

                        await command.ExecuteNonQueryAsync();
                    }
                }
                logMessage = $"Delete Success: {jsonString}";
                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                logMessage = $"Delete Failed: {ex.Message}";
                return StatusCode(500, new { message = "Delete failed", error = ex.Message });
            }
            finally
            {
                await LogExecution("DeleteMyOfficeACPD", logMessage);
            }
        }

        /// <summary>
        /// 記錄執行 Log (透過 Stored Procedure `usp_AddLog`)
        /// </summary>
        private async Task LogExecution(string procedureName, string message)
        {
            using (var connection = (SqlConnection)_context.Database.GetDbConnection())
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("usp_AddLog", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ProcedureName", procedureName));
                    command.Parameters.Add(new SqlParameter("@ErrorMessage", message));

                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
