namespace HNB.Areas.HNB_WEB.Services;

public class UserServices
{
    private HNB.Areas.HNB_WEB.Repositories.UserRepositories _userRepositories;
    
    public UserServices(HNB.Areas.HNB_WEB.Repositories.UserRepositories userRepositories)
        => _userRepositories = userRepositories;

    public void ViewBagModelUser(dynamic viewBag)
    {
        //viewBag.DepartmentList = _userRepositories.SysDepartmentList();
        viewBag.DepartmentList = null;
    }

}
