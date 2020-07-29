% Convert pixel coordinates to field coordinates
% actual dimensions of field, in cm
L = 335;
W = 300;
% pixel coordinates of the field corners
% xposRed, yposRed, ....GBY
I = [582, 362; 298, 422; 228, 92; 520, 38];
% transformation coefficients
a = [I(3, 1); I(4, 1) - I(3, 1); I(2, 1) - I(3, 1); ...
    I(1, 1) + I(3, 1) - I(2, 1) - I(4, 1)];
b = [I(3, 2); I(4, 2) - I(3, 2); I(2, 2) - I(3, 2); ...
    I(1, 2) + I(3, 2) - I(2, 2) - I(4, 2)];
% pixel coordinates of the ball
c = [393, 204];
% quadratic coefficients
Q = [a(2) * b(4) - a(4) * b(2), ...
    a(2) * b(3) - a(3) * b(2) + a(1) * b(4) - a(4)* b(1) ...
    + c(2) * a(4) - c(1) * b(4), ...
    a(1) * b(3) - a(3) * b(1) + c(2)* a(3) - c(1) * b(3)];
p = (-Q(2) - sqrt(Q(2)^2 - 4 * Q(1) * Q(3))) / (2 * Q(1))
q = (c(1) - a(1) - a(2) * p) / (a(3) + a(4) * p)
% p2 = (-Q(2) + sqrt(Q(2)^2 - 4 * Q(1) * Q(3))) / (2 * Q(1))
% q2 = (c(1) - a(1) - a(2) * p2) / (a(3) + a(4) * p2)
% check result
ball = I(3, :) + p * (I(4, :) - I(3, :)) + ...
    q * (I(2, :) - I(3, :)) + ...
    p * q * (I(1, :) + I(3, :) - I(2, :) - I(4, :))
% ball2 = I(3, :) + p2 * (I(4, :) - I(3, :)) + ...
%     q2 * (I(2, :) - I(3, :)) + ...
%     p2 * q2 * (I(1, :) + I(3, :) - I(2, :) - I(4, :))
% real position
x = p * L
y = q * W
% x2 = p2 * L
% y2 = q2 * W
return
