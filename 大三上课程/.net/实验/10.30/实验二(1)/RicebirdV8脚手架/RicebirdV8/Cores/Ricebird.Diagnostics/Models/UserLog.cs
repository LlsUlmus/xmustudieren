using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Ricebird.Diagnostics.Models
{
    [Index(nameof(CreatedOn), AllDescending = true)]
    public class UserLog
    {
        public Guid ID
        {
            get; set;
        } = SequentialGuid.NewSuid();

        [StringLength(2)]
        public string Operation
        {
            get; set;
        } = "未知";

        public Guid UserID
        {
            get; set;
        } = Guid.Empty;

        public string RealName
        {
            get; set;
        } = string.Empty;

        public string RealIp
        {
            get; set;
        } = string.Empty;

        public string CreatedBy
        {
            get; set;
        } = string.Empty;

        public DateTime CreatedOn
        {
            get; set;
        } = DateTime.Now;

        public string Decription
        {
            get; set;
        } = string.Empty;

        public string UserAgent
        {
            get; set;
        } = string.Empty;
    }
}
