using Microsoft.EntityFrameworkCore;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.HnbBackoffice.Repositories;

public class UserManagementRepositories(HnbHnbBackofficeDbContext hnb)
{
    #region 統一的查詢屬性
   
    private IQueryable<user_profile> ValidUserProfiles => hnb.user_profiles;
    private IQueryable<department_closure_v> department_Closure_Vs => hnb.department_closure_vs;
    private IQueryable<person_relation_v> person_Relation_Vs => hnb.person_relation_vs.OrderBy(p => p.person_id);

    #endregion

    public void UpdateUserProfile(user_profile model)
    {
        var entity = ValidUserProfiles.FirstOrDefault(x => x.id == model.id);

        if (model.id == 0 || entity == null)
        {
            model.extension ??= string.Empty;
            if (string.IsNullOrWhiteSpace(model.cr_user)) model.cr_user = "system";

            hnb.user_profiles.Add(model);
            hnb.SaveChanges();
            return;
        }

        // 基本識別
        entity.alias = model.alias;

        // 姓名
        entity.name = model.name;
        entity.name2 = model.name2;

        // 部門/職務
        entity.deptid = model.deptid;
        entity.titleid = model.titleid;
        entity.titlename = model.titlename;
        entity.level = model.level;
        entity.positioncode = model.positioncode;
        entity.positioncode2 = model.positioncode2;

        // 代理/外出
        entity.agentid = model.agentid;
        entity.agentstarttime = model.agentstarttime;
        entity.agentendtime = model.agentendtime;
        entity.away = model.away;

        // 聯絡
        entity.email = model.email;
        entity.phone = model.phone;
        entity.mobile = model.mobile;
        entity.comp_phone = model.comp_phone;
        entity.extension = model.extension ?? string.Empty;

        // 人資屬性
        entity.costcenter = model.costcenter;
        entity.notifytype = model.notifytype;
        entity.pluralism = model.pluralism;
        entity.manager = model.manager;
        entity.isdeptmanager = model.isdeptmanager;
        entity.status = model.status;
        entity.account = model.account;
        entity.password = model.password;
        entity.salts = model.salts;

        entity.pid = model.pid;
        entity.birthday = model.birthday;
        entity.constellation = model.constellation;
        entity.sex = model.sex;
        entity.job_status = model.job_status;

        entity.regest_date = model.regest_date;
        entity.callin_date = model.callin_date;
        entity.callout_date = model.callout_date;
        entity.replace_date = model.replace_date;
        entity.leave_date = model.leave_date;
        entity.leave_reason = model.leave_reason;

        // 公司歸屬
        entity.comp_cde = model.comp_cde;
        entity.area_cde = model.area_cde;
        entity.center = model.center;

        // 其他
        entity.job_type = model.job_type;
        entity.title1 = model.title1;
        entity.title2 = model.title2;
        entity.capital_position1 = model.capital_position1;
        entity.capital_position2 = model.capital_position2;
        entity.cp_date = model.cp_date;
        entity.position1 = model.position1;
        entity.position2 = model.position2;
        entity.position_date = model.position_date;

        entity.nationality = model.nationality;
        entity.blood_type = model.blood_type;

        entity.work_place = model.work_place;
        entity.erp_id = model.erp_id;
        entity.workers = model.workers;

        entity.userstamp = string.IsNullOrWhiteSpace(model.userstamp) ? "system" : model.userstamp;

        hnb.SaveChanges();
    }

    #region 用戶管理

    public List<person_relation_v> QueryPersonrelation(person_relation_v model)
    {
        var query = person_Relation_Vs.AsQueryable();

        query = query.Where(p =>
            (!model.person_id.HasValue || p.person_id.ToString().Contains(model.person_id.Value.ToString())) &&
            (string.IsNullOrWhiteSpace(model.person_dept_id) || p.person_dept_id.Contains(model.person_dept_id)) &&
            (string.IsNullOrWhiteSpace(model.person_name) || p.person_name.Contains(model.person_name)) &&
            (!model.related_person_id.HasValue || p.related_person_id.ToString().Contains(model.related_person_id.Value.ToString())) &&
            (string.IsNullOrWhiteSpace(model.related_dept_id) || p.related_dept_id.Contains(model.related_dept_id)) &&
            (string.IsNullOrWhiteSpace(model.related_person_name) || p.related_person_name.Contains(model.related_person_name)) &&
            (string.IsNullOrWhiteSpace(model.relation) || p.relation.Contains(model.relation))
        );


        return query.ToList();
    }

    #endregion

}
