﻿using HRSystem.Csharp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRSystem.Csharp.Domain.Models;
using HRSystem.Csharp.Domain.Models.Roles;

namespace HRSystem.Csharp.Domain.Features.Roles
{
    public class DA_Role
    {
        private readonly AppDbContext _appDbContext;

        public DA_Role(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public Result<List<RoleResponseModel>> GetAllRoles()
        {
            try
            {
                var roles = _appDbContext.TblRoles
                    .Where(r=> r.DeleteFlag != true)
                    .Select(r => new RoleResponseModel
                    {
                        RoleName = r.RoleName,
                        RoleCode = r.RoleCode,
                        RoleId = r.RoleId,
                        UniqueName = r.UniqueName,
                        CreatedAt = r.CreatedAt,
                        CreatedBy = r.CreatedBy,
                        ModifiedAt = r.ModifiedAt,
                        ModifiedBy = r.ModifiedBy,
                        DeleteFlag = r.DeleteFlag,

                    })
                    .ToList();
                return Result<List<RoleResponseModel>>.Success(roles);
            }
            catch (Exception ex)
            {
                return Result<List<RoleResponseModel>>.Error($"An error occurred while retrieving roles: {ex.Message}");
            }
        }

        public Result<bool> CreateRole(RoleRequestModel role)
        {
            try
            {
                var existing = _appDbContext.TblRoles.FirstOrDefault(r => r.RoleCode == role.RoleCode && r.DeleteFlag != true);
                if (existing != null)
                {
                    return Result<bool>.InvalidDataError("A duplicate role has already been created!");
                }

                TblRole newRole = new TblRole()
                {
                    RoleId = Ulid.NewUlid().ToString(),
                    RoleName = role.RoleName,
                    RoleCode = role.RoleCode,
                    UniqueName = role.UniqueName,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "admin",
                    ModifiedAt = null,
                    ModifiedBy = null,
                    DeleteFlag = false
                };
                _appDbContext.TblRoles.Add(newRole);
                var result = _appDbContext.SaveChanges();
                return result > 0 ? Result<bool>.Success(true)
                        : Result<bool>.Error();

            }
            catch (Exception ex)
            {
                return Result<bool>.Error($"An error occurred while creating role: {ex.Message}");
            }
        }

        public Result<RoleResponseModel> GetRoleByCode(string roleCode)
        {

            try
            {
                var role = _appDbContext.TblRoles.Where(r => r.DeleteFlag != true)
                    .Select(r => new RoleResponseModel()
                    {
                        RoleName = r.RoleName,
                        RoleCode = r.RoleCode,
                        RoleId = r.RoleId,
                        UniqueName = r.UniqueName,
                        CreatedAt = r.CreatedAt,
                        CreatedBy = r.CreatedBy,
                        ModifiedAt = r.ModifiedAt,
                        ModifiedBy = r.ModifiedBy,
                        DeleteFlag = r.DeleteFlag
                    }).FirstOrDefault(r => r.RoleCode == roleCode);

                if(role == null) return Result<RoleResponseModel>.NotFoundError("Cannot find role with given code");

                return Result<RoleResponseModel>.Success(role);

            } catch(Exception ex)
            {
                return Result<RoleResponseModel>.Error($"An error occurred while retrieving roles: {ex.Message}");

            }

        }

        public Result<bool> UpdateRole(RoleUpdateRequestModel role, string roleCode)
        {
            var existingRole = _appDbContext.TblRoles.FirstOrDefault(r => r.RoleCode == roleCode && r.DeleteFlag != true);
            if (existingRole is null) return Result<bool>.NotFoundError("Cannot find the role to be updated");
            if (role is null) return Result<bool>.InvalidDataError("Data to update role cannot be empty");

            existingRole.RoleName = role.RoleName;
            existingRole.UniqueName = role.UniqueName;
            existingRole.ModifiedAt = DateTime.Now;
            existingRole.ModifiedBy = "admin";

            var result = _appDbContext.SaveChanges();
            return result > 0 ? Result<bool>.Success(true)
                    : Result<bool>.Error();

        }

        public Result<bool> DeleteRole(string roleCode)
        {
            var roleToBeDeleted = _appDbContext.TblRoles.FirstOrDefault(r => r.RoleCode == roleCode && r.DeleteFlag != true);
            if (roleToBeDeleted is null) return Result<bool>.NotFoundError("Cannot find the role to be deleted");

            roleToBeDeleted.DeleteFlag = true;
            var result = _appDbContext.SaveChanges();
            return result > 0 ? Result<bool>.Success(true)
                    : Result<bool>.Error();

        }

    }
}
