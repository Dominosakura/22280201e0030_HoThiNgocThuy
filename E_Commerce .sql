CREATE DATABASE DECOMMERCE
GO

USE DECOMMERCE
GO

CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),  
    Username NVARCHAR(50) NOT NULL,
    Password NVARCHAR(255) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    FullName NVARCHAR(100),
    Role NVARCHAR(20) CHECK (Role IN ('Admin', 'Seller', 'Buyer')),  
    PhoneNumber NVARCHAR(20),
    Address NVARCHAR(255)
);


ALTER TABLE Users
ADD AvatarPath NVARCHAR(255);

ALTER TABLE Users
ADD CanAdmin BIT DEFAULT 0,   
    CanBuy BIT DEFAULT 0,      
    CanSell BIT DEFAULT 0;    

UPDATE Users
SET CanAdmin = CASE WHEN Role = 'Admin' THEN 1 ELSE 0 END,
    CanBuy = CASE WHEN Role = 'Buyer' THEN 1 ELSE 0 END,
    CanSell = CASE WHEN Role = 'Seller' THEN 1 ELSE 0 END;


SELECT * 
FROM Users
WHERE CanBuy IS NULL OR CanSell IS NULL;

UPDATE Users
SET CanBuy = 0, CanSell = 0
WHERE CanBuy IS NULL OR CanSell IS NULL;

  
CREATE TABLE ProductCategories (
    CategoryID INT PRIMARY KEY IDENTITY(1,1), 
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255)
);

ALTER TABLE ProductCategories
ALTER COLUMN Description NVARCHAR(MAX);

ALTER TABLE ProductCategories
ADD CreatedBy NVARCHAR(100) NULL DEFAULT 'System',   
    UpdatedBy NVARCHAR(100) NULL,                    
    CreatedDate DATETIME DEFAULT GETDATE(),           
    UpdatedDate DATETIME DEFAULT NULL;     

CREATE TABLE Products (
    ProductID INT PRIMARY KEY IDENTITY(1,1),  
    SellerID INT,  
    ProductName NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(10, 2),
    StockQuantity INT,
    CategoryID INT,  
    CreatedDate DATE,
    Status NVARCHAR(20) CHECK (Status IN ('Active', 'Inactive')),  
    FOREIGN KEY (SellerID) REFERENCES Users(UserID),
    FOREIGN KEY (CategoryID) REFERENCES ProductCategories(CategoryID)
);




CREATE TABLE ProductImages (
    ImageID INT PRIMARY KEY IDENTITY(1,1),  
    ProductID INT,  
    ImageURL NVARCHAR(255) NOT NULL,
    IsMainImage BIT DEFAULT 0,  
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);


CREATE TABLE Favorites (
    FavoriteID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT,  
    ProductID INT,  
    AddedDate DATE NOT NULL,
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);

CREATE TABLE Orders (
    OrderID INT PRIMARY KEY IDENTITY(1,1), 
    BuyerID INT,  
    OrderDate DATE NOT NULL,
    TotalAmount DECIMAL(10, 2),
    ShippingAddress NVARCHAR(255),
    Status NVARCHAR(20) CHECK (Status IN ('Pending', 'Shipped', 'Delivered', 'Cancelled')), 
    FOREIGN KEY (BuyerID) REFERENCES Users(UserID)
);

ALTER TABLE Orders
ADD RecipientName NVARCHAR(100);
ALTER TABLE Orders
ADD RecipientEmail NVARCHAR(100);
ALTER TABLE Orders
ADD RecipientPhonumber NVARCHAR(20);

ALTER TABLE Orders
DROP CONSTRAINT CK__Orders__Status__44FF419A;

UPDATE Orders
SET Status = 'Pending Confirmation'  
WHERE Status = 'Pending';  

SELECT DISTINCT Status
FROM Orders;

UPDATE Orders
SET Status = 'Delivered'  
WHERE Status = 'Shipped'; 

ALTER TABLE Orders
ADD CONSTRAINT CK_Orders_Status CHECK (Status IN ('Pending Confirmation', 'Awaiting Pickup', 'Awaiting Delivery', 'Delivered', 'Reviewed', 'Cancelled'));

CREATE TABLE OrderDetails (
    OrderDetailID INT PRIMARY KEY IDENTITY(1,1),  
    OrderID INT,  
    ProductID INT,  
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10, 2),
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);

CREATE TABLE Reviews (
    ReviewID INT PRIMARY KEY IDENTITY(1,1),  
    ProductID INT,  
    UserID INT,  
    Rating INT CHECK (Rating BETWEEN 1 AND 5),  
    Comment NVARCHAR(MAX),
    ReviewDate DATE NOT NULL,
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

CREATE TABLE ShoppingCart (
    CartID INT PRIMARY KEY IDENTITY(1,1),  
    UserID INT,  
    CreatedDate DATE NOT NULL,
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

CREATE TABLE ShoppingCartItems (
    CartItemID INT PRIMARY KEY IDENTITY(1,1),  
    CartID INT,  
    ProductID INT,  
    Quantity INT NOT NULL,
    FOREIGN KEY (CartID) REFERENCES ShoppingCart(CartID),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);

CREATE TABLE Payments (
    PaymentID INT PRIMARY KEY IDENTITY(1,1), 
    OrderID INT,  
    PaymentDate DATE NOT NULL,
    PaymentMethod NVARCHAR(50) CHECK (PaymentMethod IN (
      'COD', 'E-wallet', 'Credit Card'    )),  
    PaymentAmount DECIMAL(10, 2),
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID)
);



CREATE TABLE Shipping (
    ShippingID INT PRIMARY KEY IDENTITY(1,1),  
    OrderID INT,  
    ShippingMethod NVARCHAR(20) CHECK (ShippingMethod IN ('Standard', 'Express')),  
    ShippingCost DECIMAL(10, 2),
    EstimatedDeliveryDate DATE,
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID)
);


CREATE TABLE Promotion (
    PromotionID INT PRIMARY KEY IDENTITY(1,1),  
    SellerID INT NULL,  -- Áp dụng cho toàn bộ sản phẩm của người bán (NULL nếu không cần)
    ProductID INT NULL, -- Áp dụng cho sản phẩm cụ thể (NULL nếu không cần)
    DiscountPercentage FLOAT CHECK (DiscountPercentage BETWEEN 0 AND 100), -- % giảm giá
    StartDate DATETIME NOT NULL,  
    EndDate DATETIME NOT NULL,  
    FOREIGN KEY (SellerID) REFERENCES Users(UserID),  
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);


INSERT INTO Users (Username, Password, Email, FullName, Role, PhoneNumber, Address)
VALUES 
(N'A_Hien', 'hashed_password_admin', 'admin@example.com', 'Nguyễn Minh Hiền', 'Admin', '0123456789', N'Hồ Chí Minh'),
(N'S_Thai', 'hashed_password_seller1', 'seller1@example.com', 'Anh Ngọc Thái', 'Seller', '0987654321', N'Bình Dương'),
(N'B_Huong', 'hashed_password_buyer1', 'buyer1@example.com', 'Trần Thị Thu Hương', 'Buyer', '0123456789', N'Thái Bình'),
(N'B_Anh', 'hashed_password_user1', 'user1@example.com', 'Nguyễn Văn Anh', 'Buyer', '0123456780', N'Bình Dương'),
(N'B_Binh', 'hashed_password_user2', 'user2@example.com', 'Lê Thị Bình', 'Buyer', '0123456781', N'Bình Dương');

INSERT INTO Users (Username, Password, Email, FullName, Role, PhoneNumber, Address)
VALUES 
(N'S_Lan', 'hashed_password_seller2', 'seller2@example.com', 'Lê Thị Lan', 'Seller', '0971234567', N'Hà Nội'),
(N'B_Hoa', 'hashed_password_user3', 'user3@example.com', 'Nguyễn Thị Hoa', 'Buyer', '0977654321', N'Hà Nội');


INSERT INTO ProductCategories (CategoryName, Description)
VALUES 
(N'Men''s Fashion', N'Quần áo, phụ kiện thời trang cho nam'),
(N'Mobile Phones & Accessories', N'Các sản phẩm như điện thoại, máy tính'),
(N'Electronics', N'Các phụ kiện điện tử như tai nghe, cáp sạc'),
(N'Computers & Laptops', N'Các loại máy tính và laptop'),
(N'Cameras & Camcorders', N'Máy ảnh và máy quay phim'),
(N'Watches', N'Các loại đồng hồ'),
(N'Beauty', N'Sản phẩm làm đẹp'),
(N'Women''s Fashion', N'Quần áo, phụ kiện thời trang cho nữ'),
(N'Accessories & Jewelry', N'Phụ kiện và trang sức'),
(N'Bookstore', N'Sách và văn phòng phẩm'),
(N'Sports & Travel', N'Dụng cụ thể thao và du lịch'),
(N'Children''s Fashion', N'Thời trang trẻ em'),
(N'Toys', N'Đồ chơi'),
(N'Pet Care', N'Sản phẩm chăm sóc thú cưng'),
(N'Health', N'Sản phẩm chăm sóc sức khỏe'),
(N'Others', N'Các sản phẩm khác');



INSERT INTO Products (SellerID, ProductName, Description, Price, StockQuantity, CategoryID, CreatedDate, Status)
VALUES 
(2, N'Smartphone X', N'Điện thoại thông minh với nhiều tính năng hiện đại', 999.99, 100, 1, '2024-09-12', 'Active'),
(2, N'Leather Jacket', N'Áo khoác da thời trang cho mùa đông', 149.99, 50, 2, '2024-09-10', 'Active'),
(2, N'USB Cable', N'Cáp sạc USB chất lượng cao', 9.99, 200, 3, '2024-09-08', 'Active');

INSERT INTO Products (SellerID, ProductName, Description, Price, StockQuantity, CategoryID, CreatedDate, Status)
VALUES 
(2, N'Laptop Y', N'Laptop với hiệu năng cao cho lập trình', 1200.50, 30, 4, '2024-09-15', 'Active'),
(2, N'DSLR Camera', N'Máy ảnh kỹ thuật số chuyên nghiệp', 899.99, 20, 5, '2024-09-16', 'Active'),
(4, N'Smartwatch', N'Đồng hồ thông minh', 199.99, 150, 6, '2024-09-17', 'Active'),
(4, N'Makeup Kit', N'Bộ trang điểm chất lượng cao', 79.99, 80, 7, '2024-09-18', 'Active'),
(4, N'Fashionable Dress', N'Đầm thời trang cho nữ', 99.99, 120, 8, '2024-09-19', 'Active');


INSERT INTO ProductImages (ProductID, ImageURL, IsMainImage)
VALUES 
(1, 'SmartPhoneXC.jpg', 1),  -- Ảnh chính của Smartphone X
(1, 'SmartphoneXP.jpg', 0),     -- Ảnh phụ của Smartphone X
(2, 'AoKhoacDa .jpg', 1),      -- Ảnh chính của áo khoác da
(3, 'USB.jpg', 1);    -- Ảnh chính của cáp sạc USB

INSERT INTO ProductImages (ProductID, ImageURL, IsMainImage)
VALUES 
(4, 'Laptop.jpg', 1),
(4, 'Laptop1.jpg', 0),
(5, 'Camera.jpg', 1),
(6, 'smartwatch.jpg', 1),
(7, 'makeup_kit.jpg', 1),
(8, 'dressjpg.jpg', 1);

INSERT INTO Orders (BuyerID, OrderDate, TotalAmount, ShippingAddress, Status)
VALUES 
(3, '2024-09-12', 999.99, N'789 Lê Hồng Phong, Phú Hòa, Thủ Dầu Một', 'Pending');

INSERT INTO Orders (BuyerID, OrderDate, TotalAmount, ShippingAddress, Status)
VALUES 
(5, '2024-09-16', 1200.50, N'456 Nguyễn Trãi, Thanh Xuân, Hà Nội', 'Shipped'),
(3, '2024-09-17', 199.99, N'123 Trần Hưng Đạo, Quận 1, Hồ Chí Minh', 'Pending');

INSERT INTO OrderDetails (OrderID, ProductID, Quantity, UnitPrice)
VALUES 
(1, 1, 1, 999.99);  -- 1 chiếc Smartphone X

INSERT INTO OrderDetails (OrderID, ProductID, Quantity, UnitPrice)
VALUES 
(2, 4, 1, 1200.50),  -- 1 chiếc Laptop Y
(3, 6, 2, 199.99);  -- 2 chiếc Smartwatch

INSERT INTO Reviews (ProductID, UserID, Rating, Comment, ReviewDate)
VALUES 
(4, 5, 5, N'Máy tính chạy rất mượt, hài lòng!', '2024-09-16'),
(5, 3, 4, N'Máy ảnh chụp đẹp nhưng hơi nặng.', '2024-09-18');

INSERT INTO Reviews (ProductID, UserID, Rating, Comment, ReviewDate)
VALUES 
(1, 3, 5, N'Điện thoại rất tốt, mình rất hài lòng!', '2024-09-15'),
(2, 3, 4, N'Áo khoác chất lượng, nhưng hơi đắt.', '2024-09-13');



-- Inserting into ShoppingCart
INSERT INTO ShoppingCart (UserID, CreatedDate)
VALUES 
(3, '2024-09-11'),  -- Shopping cart for Buyer 1
(4, '2024-09-12');  -- Shopping cart for Buyer 2


INSERT INTO ShoppingCartItems (CartID, ProductID, Quantity)
VALUES 
(1, 5, 1),  -- 1 chiếc Máy ảnh DSLR trong giỏ hàng của Buyer 1
(1, 4, 1);  -- 1 chiếc Laptop Y trong giỏ hàng của Buyer 1

INSERT INTO ShoppingCartItems (CartID, ProductID, Quantity)
VALUES 
(1, 3, 2);  -- 2 chiếc cáp sạc USB trong giỏ hàng của Buyer 1

INSERT INTO Payments (OrderID, PaymentDate, PaymentMethod, PaymentAmount)
VALUES 
(1, '2024-09-12', 'CreditCard', 999.99);

INSERT INTO Payments (OrderID, PaymentDate, PaymentMethod, PaymentAmount)
VALUES 
(2, '2024-09-16', 'PayPal', 1200.50),
(3, '2024-09-17', 'CreditCard', 199.99);

-- Additional Shipping
INSERT INTO Shipping (OrderID, ShippingMethod, ShippingCost, EstimatedDeliveryDate)
VALUES 
(2, 'Express', 10.00, '2024-09-19'),
(3, 'Standard', 5.00, '2024-09-22');

INSERT INTO Shipping (OrderID, ShippingMethod, ShippingCost, EstimatedDeliveryDate)
VALUES 
(1, 'Standard', 5.00, '2024-09-17');

SELECT 
    OBJECT_NAME(OBJECT_ID) AS TableName,
    name AS ConstraintName,
    definition
FROM 
    sys.check_constraints
WHERE 
    parent_object_id = OBJECT_ID('dbo.Orders');



	SELECT TOP 3 p.ProductID, p.ProductName, p.Price, p.StockQuantity, c.CategoryName
FROM Products p
JOIN OrderDetails od ON p.ProductID = od.ProductID
JOIN Orders o ON od.OrderID = o.OrderID
JOIN ProductCategories c ON p.CategoryID = c.CategoryID
GROUP BY p.ProductID, p.ProductName, p.Price, p.StockQuantity, c.CategoryName
ORDER BY SUM(od.Quantity) DESC;

SELECT COUNT(*) FROM Products;
SELECT COUNT(*) FROM OrderDetails;
SELECT COUNT(*) FROM Orders;
SELECT COUNT(*) FROM ProductCategories;

SELECT * FROM OrderDetails WHERE Quantity > 0;


SELECT TOP 3 p.ProductID, p.ProductName, p.Price, p.StockQuantity, c.CategoryName
FROM Products p
LEFT JOIN OrderDetails od ON p.ProductID = od.ProductID
LEFT JOIN Orders o ON od.OrderID = o.OrderID
LEFT JOIN ProductCategories c ON p.CategoryID = c.CategoryID
GROUP BY p.ProductID, p.ProductName, p.Price, p.StockQuantity, c.CategoryName
ORDER BY SUM(COALESCE(od.Quantity, 0)) DESC;

SELECT TOP 3 ProductID, SUM(Quantity) as TotalQuantity
FROM OrderDetails
GROUP BY ProductID
ORDER BY TotalQuantity DESC;

SELECT COUNT(*) FROM OrderDetails;

UPDATE od
SET 
    od. = o.OrderDate,
    od.CustomerID = o.CustomerID,
    od.TotalAmount = o.TotalAmount
FROM OrderDetails od
INNER JOIN [Orders] o ON od.OrderID = o.OrderID;
