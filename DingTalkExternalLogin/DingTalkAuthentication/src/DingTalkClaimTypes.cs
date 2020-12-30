namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 钉钉特有的 Claim
    /// </summary>
    public static class DingTalkClaimTypes
    {
        /// <summary>
        /// 员工在当前开发者企业账号范围内的唯一标识
        /// </summary>
        public const string UnionId = "urn:dingtalk:unionid";

        /// <summary>
        /// 员工工号
        /// </summary>
        public const string JobNumber = "urn:dingtalk:jobno";

        /// <summary>
        /// 职位信息
        /// </summary>
        public const string Position = "urn:dingtalk:position";

        /// <summary>
        /// 是否是高管
        /// </summary>
        public const string IsSenior = "urn:dingtalk:is_senior";

        /// <summary>
        /// 员工的企业邮箱
        /// </summary>
        public const string OrgEmail= "urn:dingtalk:ogr_email";

        /// <summary>
        /// 是否实名认证
        /// </summary>
        public const string RealAuthed = "urn:dingtalk:real_authed";

        /// <summary>
        /// 是否是老板
        /// </summary>
        public const string IsBoss = "urn:dingtalk:is_boss";

        /// <summary>
        /// 是否为企业的管理员
        /// </summary>
        public const string IsAdmin = "urn:dingtalk:is_admin";

        /// <summary>
        /// 在对应的部门中是否为主管：Map结构的json字符串，key是部门的id，value是人员在这个部门中是否为主管，true表示是，false表示不是
        /// </summary>
        public const string IsLeaderInDepts = "urn:dingtalk:is_leader_in_depts";

        /// <summary>
        /// 入职时间。Unix时间戳
        /// </summary>
        public const string HiredDate = "urn:dingtalk:hired_date";

        /// <summary>
        /// 扩展属性
        /// </summary>
        public const string Extattr = "urn:dingtalk:ext_attr";
    }

}
